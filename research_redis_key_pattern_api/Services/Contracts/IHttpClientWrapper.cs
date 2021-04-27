using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace research_redis_key_pattern_api.Services.Contracts
{

    /// <summary>
    /// <see cref="Rosen.CoreLib.Utility.Http.HttpRequestApiHelper" /> wrapper.
    /// </summary>
    public interface IHttpClientWrapper
    {
        /// <summary>
        /// Get asynchronous string from api.
        /// </summary>
        /// <param name="client">before using this method should wrap them in a http client.</param>
        /// <param name="apiUrl">api url.</param>
        /// <returns>by Json any type of object or exception if can not response.</returns>
        Task<T> GetStringFromApiAsync<T>(HttpClient client, string apiUrl);
    }
}
