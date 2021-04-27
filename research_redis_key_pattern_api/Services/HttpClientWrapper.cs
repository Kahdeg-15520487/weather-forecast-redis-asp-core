using Newtonsoft.Json;

using research_redis_key_pattern_api.Services.Contracts;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace research_redis_key_pattern_api.Services
{
    public class HttpClientWrapper : IHttpClientWrapper
    {
        /// <inheritdoc/>
        public async Task<T> GetStringFromApiAsync<T>(HttpClient httpClient, string apiUrl)
        {
            return (T)(await httpClient.GetObjectFromApiAsync<T>(apiUrl)).Obj;
        }
    }

    public class ResponseObject
    {
        /// <summary>
        /// Gets or sets return object data.
        /// </summary>
        public object Obj { get; set; }

        /// <summary>
        /// Gets or sets response message include reponse status code.
        /// </summary>
        public HttpResponseMessage ResponseMessage { get; set; }
    }

    public static class HttpRequestApiHelper
    {
        /// <summary>
        /// get asynchronous object from api.
        /// </summary>
        /// <param name="client">before using this method should wrap them in a http client.</param>
        /// <param name="apiUrl">api url.</param>
        /// <returns>by Json any type of object or exception if can not response.</returns>
        public static async Task<ResponseObject> GetObjectFromApiAsync<T>(this HttpClient client, string apiUrl)
        {
            var responseObj = new ResponseObject
            {
                ResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError),
            };

            try
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    responseObj.Obj = JsonConvert.DeserializeObject<T>(data);
                }

                responseObj.ResponseMessage = response;
            }
            catch (Exception ex)
            {
                responseObj.ResponseMessage.Content = new StringContent($"Exception: {ex.GetType().ToString()}", Encoding.UTF8, "application/json");
            }

            return responseObj;
        }
    }
}
