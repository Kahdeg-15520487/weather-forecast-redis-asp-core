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

    /// <summary>
    /// Wrapper for calling extension methods for setting data with <see cref="StackExchange.Redis.IDatabase"/>.
    /// </summary>
    public interface IDistributedCacheWrapper
    {
        /// <summary>
        /// Get a string.
        /// </summary>
        /// <param name="key">key.</param>
        /// <returns>value.</returns>
        string StringGet(string key);
        /// <summary>
        /// Get a string asynchronously.
        /// </summary>
        /// <param name="key">key.</param>
        /// <returns>Task of getting value.</returns>
        Task<string> StringGetAsync(string key);
        /// <summary>
        /// Get many strings.
        /// </summary>
        /// <param name="keys">keys.</param>
        /// <returns>values.</returns>
        IEnumerable<string> StringGetMany(string[] keys);
        /// <summary>
        /// Get many strings asynchronously.
        /// </summary>
        /// <param name="keys">keys.</param>
        /// <returns>Task of getting many values.</returns>
        Task<IEnumerable<string>> StringGetManyAsync(string[] keys);

        /// <summary>
        /// Set a string.
        /// </summary>
        /// <param name="key">key.</param>
        /// <param name="value">value.</param>
        void StringSet(string key, string value);
        /// <summary>
        /// Set a string with expiry time.
        /// </summary>
        /// <param name="key">key.</param>
        /// <param name="value">value.</param>
        /// <param name="expiryTime">cache expiry time.</param>
        void StringSet(string key, string value, TimeSpan expiryTime);
        /// <summary>
        /// Set a string asynchronously.
        /// </summary>
        /// <param name="key">key.</param>
        /// <param name="value">value.</param>
        Task StringSetAsync(string key, string value);
        /// <summary>
        /// Set a string asynchronously with expiry time.
        /// </summary>
        /// <param name="key">key.</param>
        /// <param name="value">value.</param>
        /// <param name="expiryTime">cache expiry time.</param>
        Task StringSetAsync(string key, string value, TimeSpan expiryTime);

        /// <summary>
        /// Get a Hash asynchronously.
        /// </summary>
        /// <param name="key">Hash's key.</param>
        /// <returns>Hash's fields.</returns>
        Task<IEnumerable<KeyValuePair<string, string>>> HashGet(string key);
        /// <summary>
        /// Set a Hash asynchronously.
        /// </summary>
        /// <param name="key">Hash's key.</param>
        /// <param name="fields">Hash's fields.</param>
        /// <returns><see cref="Task"/></returns>
        Task HashSet(string key, IEnumerable<KeyValuePair<string, string>> fields);

        /// <summary>
        /// Add values to a set asynchronously.
        /// </summary>
        /// <param name="key">Set's key.</param>
        /// <param name="values">Value to be added.</param>
        /// <returns><see cref="Task"/></returns>
        Task SetAdd(string key, params string[] values);
        /// <summary>
        /// Get values from a set asynchronously.
        /// </summary>
        /// <param name="key">Set's key.</param>
        /// <returns><see cref="Task{IEnumerable{String}}"/></returns>
        Task<IEnumerable<string>> SetGet(string key);
        /// <summary>
        /// Get values from intersecting 2 sets asynchronously.
        /// </summary>
        /// <param name="key1">Set 1's key.</param>
        /// <param name="key2">Set 2's key.</param>
        /// <returns><see cref="Task{IEnumerable{String}}"/></returns>
        Task<IEnumerable<string>> SetIntersect(string key1, string key2);

        /// <summary>
        /// Add values to a sorted set asynchronously.
        /// </summary>
        /// <param name="key">Sorted set's key.</param>
        /// <param name="scoreCalc">Lambda function to calculate value's score</param>
        /// <param name="values">Value to be added.</param>
        /// <returns><see cref="Task"/></returns>
        Task SortedSetAdd(string key, Func<string, double> scoreCalc, params string[] values);
        /// <summary>
        /// Get values from a sorted set with order asynchronously.
        /// </summary>
        /// <param name="key">Sorted set's key.</param>
        /// <param name="sortOrder">Sort order.</param>
        /// <returns><see cref="Task"/></returns>
        Task<IEnumerable<string>> SortedSetGet(string key, ScoreSortOrder sortOrder = ScoreSortOrder.Ascending);
        /// <summary>
        /// Get values from intersecting 2 sorted sets asynchronously.
        /// </summary>
        /// <param name="key1">Set 1's key.</param>
        /// <param name="key2">Set 2's key.</param>
        /// <returns><see cref="Task{IEnumerable{String}}"/></returns>
        Task<IEnumerable<string>> SortedSetIntersect(string key1, string key2, ScoreSortOrder sortOrder = ScoreSortOrder.Ascending);
    }
}
