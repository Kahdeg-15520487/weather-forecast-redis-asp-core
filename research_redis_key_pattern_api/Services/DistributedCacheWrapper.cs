using Microsoft.Extensions.Caching.Distributed;

using research_redis_key_pattern.core;

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
        static readonly TimeSpan tempSetCacheExpiryTimeSpan = new TimeSpan(1, 0, 0);
        private readonly IRedisConnectionFactory redisConnectionFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DistributedCacheWrapper"/> class.
        /// </summary>
        /// <param name="redisConnectionFactory">inject an instance of <see cref="IRedisConnectionFactory"/>.</param>
        public DistributedCacheWrapper(IRedisConnectionFactory redisConnectionFactory)
        {
            this.redisConnectionFactory = redisConnectionFactory;
        }

        private IDatabase GetRedisDatabse()
        {

            return this.redisConnectionFactory.Connection().GetDatabase();
        }

        #region string
        public IEnumerable<string> StringGetMany(string[] keys)
        {
            return this.GetRedisDatabse().StringGet(keys.ToRedisKeys().ToArray()).ToStringArray();
        }

        public async Task<IEnumerable<string>> StringGetManyAsync(string[] keys)
        {
            return (await this.GetRedisDatabse().StringGetAsync(keys.ToRedisKeys().ToArray())).ToStringArray();
        }

        /// <inheritdoc/>
        public string StringGet(string key)
        {
            return this.GetRedisDatabse().StringGet(key);
        }

        /// <inheritdoc/>
        public async Task<string> StringGetAsync(string key)
        {
            return await this.GetRedisDatabse().StringGetAsync(key);
        }

        /// <inheritdoc/>
        public void StringSet(string key, string value)
        {
            this.GetRedisDatabse().StringSet(key, value);
        }

        /// <inheritdoc/>
        public void StringSet(string key, string value, TimeSpan expiryTime)
        {
            this.GetRedisDatabse().StringSet(key, value, expiryTime);
        }

        /// <inheritdoc/>
        public async Task StringSetAsync(string key, string value)
        {
            await this.GetRedisDatabse().StringSetAsync(key, value);
        }

        /// <inheritdoc/>
        public async Task StringSetAsync(string key, string value, TimeSpan expiryTime)
        {
            await this.GetRedisDatabse().StringSetAsync(key, value, expiryTime);
        }
        #endregion

        #region Hash
        public async Task HashSet(string key, IEnumerable<KeyValuePair<string, string>> fields)
        {
            await this.GetRedisDatabse().HashSetAsync(key, fields.ToHashEntries().ToArray());
        }

        public async Task<IEnumerable<KeyValuePair<string, string>>> HashGet(string key)
        {
            return (await this.GetRedisDatabse().HashGetAllAsync(key)).ToKeyValuePairs();
        }
        #endregion

        #region Set
        public async Task SetAdd(string key, params string[] values)
        {
            await this.GetRedisDatabse().SetAddAsync(key, values.ToRedisValueArray().ToArray());
        }

        public async Task<IEnumerable<string>> SetGet(string key)
        {
            return (await this.GetRedisDatabse().SetMembersAsync(key)).ToStringArray();
        }

        public async Task<IEnumerable<string>> SetIntersect(string key1, string key2)
        {
            return (await this.GetRedisDatabse().SetCombineAsync(SetOperation.Intersect, key1, key2)).ToStringArray();
        }
        #endregion

        #region SortedSet
        public async Task SortedSetAdd(string key, Func<string, double> scoreCalc, params string[] values)
        {
            await this.GetRedisDatabse().SortedSetAddAsync(key, values.ToSortedSetEntries(scoreCalc).ToArray());
        }

        public async Task<IEnumerable<string>> SortedSetGet(string key, ScoreSortOrder scoreSortOrder = ScoreSortOrder.Ascending)
        {
            return (await this.GetRedisDatabse().SortedSetRangeByRankAsync(key, order: scoreSortOrder == ScoreSortOrder.Ascending ? Order.Ascending : Order.Descending))
                    .ToStringArray();
        }

        public async Task<IEnumerable<string>> SortedSetIntersect(string key1, string key2, ScoreSortOrder scoreSortOrder = ScoreSortOrder.Ascending)
        {
            var tempCacheKey = string.Format(tempSetCacheKeyTemplate, Guid.NewGuid());
            if (!await this.GetRedisDatabse().KeyExistsAsync(tempCacheKey))
            {
                await this.GetRedisDatabse().SortedSetCombineAndStoreAsync(SetOperation.Intersect, tempCacheKey, key1, key2);
                await this.GetRedisDatabse().KeyExpireAsync(tempCacheKey, tempSetCacheExpiryTimeSpan);
            }
            return await SortedSetGet(tempCacheKey, scoreSortOrder);
        }
        #endregion
    }
}
