using Microsoft.Extensions.Caching.Distributed;

using research_redis_key_pattern_api.Services.Contracts;

using StackExchange.Redis;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace research_redis_key_pattern_api.Services
{
    public class DistributedCacheWrapper : IDistributedCacheWrapper
    {
        static readonly string tempSetCacheKeyTemplate = "temp:{0}";

        private readonly IDistributedCache distributedCache;
        private readonly IRedisConnectionFactory redisConnectionFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DistributedCacheWrapper"/> class.
        /// </summary>
        /// <param name="distributedCache">inject an instance of <see cref="IDistributedCache"/>.</param>
        public DistributedCacheWrapper(IDistributedCache distributedCache, IRedisConnectionFactory redisConnectionFactory)
        {
            this.distributedCache = distributedCache;
            this.redisConnectionFactory = redisConnectionFactory;
        }

        #region distributed cache
        /// <inheritdoc/>
        public string GetString(string key)
        {
            return this.distributedCache.GetString(key);
        }

        /// <inheritdoc/>
        public async Task<string> GetStringAsync(string key, CancellationToken cancellationToken = default)
        {
            return await this.distributedCache.GetStringAsync(key, cancellationToken);
        }

        /// <inheritdoc/>
        public void SetString(string key, string value)
        {
            this.distributedCache.SetString(key, value);
        }

        /// <inheritdoc/>
        public void SetString(string key, string value, DistributedCacheEntryOptions options)
        {
            this.distributedCache.SetString(key, value, options);
        }

        /// <inheritdoc/>
        public async Task SetStringAsync(string key, string value, CancellationToken cancellationToken = default)
        {
            await this.distributedCache.SetStringAsync(key, value, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task SetStringAsync(string key, string value, DistributedCacheEntryOptions options, CancellationToken cancellationToken = default)
        {
            await this.distributedCache.SetStringAsync(key, value, options, cancellationToken);
        }
        #endregion

        #region IDatabase

        #region GetMany
        /// <inheritdoc/>
        public async IAsyncEnumerable<string> GetStringManyAsync(string[] keys)
        {
            //TODO use redis string instead of IDistributedCache
            IDatabase db = this.redisConnectionFactory.Connection().GetDatabase();

            foreach (string key in keys)
            {
                yield return await db.HashGetAsync(key, "data");
            }
        }
        #endregion
        #region Hash
        public async Task SetHash(string key, IEnumerable<KeyValuePair<string, string>> fields)
        {
            IDatabase db = this.redisConnectionFactory.Connection().GetDatabase();

            await db.HashSetAsync(key, GetHashEntries(fields).ToArray());
        }

        public async Task<IEnumerable<KeyValuePair<string, string>>> GetHash(string key)
        {
            IDatabase db = this.redisConnectionFactory.Connection().GetDatabase();

            return GetKeyValuePairs(await db.HashGetAllAsync(key));
        }
        #endregion
        #region Set
        public async Task SetAdd(string key, params string[] values)
        {
            IDatabase db = this.redisConnectionFactory.Connection().GetDatabase();

            await db.SetAddAsync(key, GetRedisValues(values).ToArray());
        }

        public async Task<IEnumerable<string>> SetGet(string key)
        {
            IDatabase db = this.redisConnectionFactory.Connection().GetDatabase();

            return GetStrings(await db.SetMembersAsync(key));
        }

        public async Task<IEnumerable<string>> SetInter(string key1, string key2)
        {
            IDatabase db = this.redisConnectionFactory.Connection().GetDatabase();

            return GetStrings(await db.SetCombineAsync(SetOperation.Intersect, key1, key2));
        }
        #endregion
        #region SortedSet
        public async Task SortedSetAdd(string key, Func<string, double> scoreCalc, params string[] values)
        {
            IDatabase db = this.redisConnectionFactory.Connection().GetDatabase();

            await db.SortedSetAddAsync(key, GetSortedSetEntries(values, scoreCalc).ToArray());
        }

        public async Task<IEnumerable<string>> SortedSetGet(string key)
        {
            IDatabase db = this.redisConnectionFactory.Connection().GetDatabase();

            return GetStrings(await db.SortedSetRangeByRankAsync(key));
        }

        public async Task<IEnumerable<string>> SortedSetInter(string key1, string key2)
        {
            IDatabase db = this.redisConnectionFactory.Connection().GetDatabase();
            var tempCacheKey = string.Format(tempSetCacheKeyTemplate, Guid.NewGuid());
            var count = await db.SortedSetCombineAndStoreAsync(SetOperation.Intersect, tempCacheKey, key1, key2);
            return await SetGet(tempCacheKey);
        }
        #endregion
        #endregion

        #region helper
        private IEnumerable<RedisKey> GetRedisKeys(string[] values)
        {
            foreach (string v in values)
            {
                yield return v;
            }
        }

        private IEnumerable<RedisValue> GetRedisValues(string[] values)
        {
            foreach (string v in values)
            {
                yield return v;
            }
        }

        private IEnumerable<string> GetStrings(RedisValue[] values)
        {
            foreach (RedisValue v in values)
            {
                yield return v;
            }
        }

        private IEnumerable<HashEntry> GetHashEntries(IEnumerable<KeyValuePair<string, string>> fields)
        {
            foreach (KeyValuePair<string, string> kvp in fields)
            {
                yield return new HashEntry(kvp.Key, kvp.Value);
            }
        }

        private IEnumerable<KeyValuePair<string, string>> GetKeyValuePairs(IEnumerable<HashEntry> hashEntries)
        {
            foreach (HashEntry he in hashEntries)
            {
                yield return new KeyValuePair<string, string>(he.Name, he.Value);
            }
        }

        private IEnumerable<SortedSetEntry> GetSortedSetEntries(string[] members, Func<string, double> scoreCalc)
        {
            foreach (var m in members)
            {
                yield return new SortedSetEntry(m, scoreCalc(m));
            }
        }
        #endregion
    }
}
