using Microsoft.Extensions.Caching.Distributed;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace research_redis_key_pattern_api.Services.Contracts
{
    public enum ScoreSortOrder
    {
        Ascending,
        Descending
    }
    public interface IDistributedCacheWrapper
    {
        #region IDistributedCache
        /// <summary>
        /// Gets a string from the specified cache with the specified key.
        /// </summary>
        /// <param name="key">The key to get the stored data for.</param>
        /// <returns>The string value from the stored cache key.</returns>
        string GetString(string key);

        /// <summary>
        /// Asynchronously gets a string from the specified cache with the specified key.
        /// </summary>
        /// <param name="key">The key to get the stored data for.</param>
        /// <param name="cancellationToken">Optional. A System.Threading.CancellationToken to cancel the operation.</param>
        /// <returns>A task that gets the string value from the stored cache key.</returns>
        Task<string> GetStringAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets a string in the specified cache with the specified key.
        /// </summary>
        /// <param name="key">The key to store the data in.</param>
        /// <param name="value">The data to store in the cache.</param>
        void SetString(string key, string value);

        /// <summary>
        /// Sets a string in the specified cache with the specified key.
        /// </summary>
        /// <param name="key">The key to store the data in.</param>
        /// <param name="value">The data to store in the cache.</param>
        /// <param name="options"> The cache options for the entry.</param>
        void SetString(string key, string value, DistributedCacheEntryOptions options);

        /// <summary>
        /// Asynchronously sets a string in the specified cache with the specified key.
        /// </summary>
        /// <param name="key">The key to store the data in.</param>
        /// <param name="value">The data to store in the cache.</param>
        /// <param name="cancellationToken">Optional. A System.Threading.CancellationToken to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous set operation.</returns>
        Task SetStringAsync(string key, string value, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously sets a string in the specified cache with the specified key.
        /// </summary>
        /// <param name="key">The key to store the data in.</param>
        /// <param name="value">The data to store in the cache.</param>
        /// <param name="options"> The cache options for the entry.</param>
        /// <param name="token">Optional. A System.Threading.CancellationToken to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous set operation.</returns>
        Task SetStringAsync(string key, string value, DistributedCacheEntryOptions options, CancellationToken cancellationToken = default);
        #endregion

        #region IDatabase

        IAsyncEnumerable<string> GetStringManyAsync(string[] keys);
        #region Hash
        Task SetHash(string key, IEnumerable<KeyValuePair<string, string>> fields);

        Task<IEnumerable<KeyValuePair<string, string>>> GetHash(string key);
        #endregion
        #region Set
        Task SetAdd(string key, params string[] values);

        Task<IEnumerable<string>> SetGet(string key);

        Task<IEnumerable<string>> SetInter(string key1, string key2);
        #endregion
        #region SortedSet
        Task SortedSetAdd(string key, Func<string, double> scoreCalc, params string[] values);

        Task<IEnumerable<string>> SortedSetGet(string key);

        Task<IEnumerable<string>> SortedSetInter(string key1, string key2);
        #endregion
        #endregion
    }
}
