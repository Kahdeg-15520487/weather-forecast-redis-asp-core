using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using research_redis_key_pattern_api.Services.Contracts;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace research_redis_key_pattern_api.Services
{
    public class RemoteWeatherServiceProxy : IRemoteWeatherServiceProxy
    {
        static readonly string apiRequestTemplate = "api/weather/{0}";

        static readonly string cacheKeyTemplate = "weather:{0}";

        static readonly string dateTemplate = "yyyy-MM-dd";

        private readonly IDistributedCache distributedCache;
        private readonly HttpClient httpClient;
        private readonly IRedisConnectionFactory redisConnectionFactory;
        private readonly ILogger<RemoteWeatherServiceProxy> logger;

        public RemoteWeatherServiceProxy(IDistributedCache distributedCache, IHttpClientFactory httpClientFactory, IRedisConnectionFactory redisConnectionFactory, ILogger<RemoteWeatherServiceProxy> logger)
        {
            this.distributedCache = distributedCache;
            this.httpClient = httpClientFactory.CreateClient("remote");
            this.redisConnectionFactory = redisConnectionFactory;
            this.logger = logger;
        }

        public async Task<WeatherForecast> GetWeather(DateTime date)
        {
            DateTime dateWithoutTime = date.Date;

            string cacheKey = string.Format(cacheKeyTemplate, dateWithoutTime.Second);

            string cachedWeatherForecast = await this.distributedCache.GetStringAsync(cacheKey);
            if (string.IsNullOrWhiteSpace(cachedWeatherForecast))
            {
                logger.LogInformation("{0} havent been fetched.", cacheKey);
                string request = string.Format(apiRequestTemplate, date);
                string newWeatherForecast = await this.httpClient.GetStringAsync(request);
                await this.distributedCache.SetStringAsync(cacheKey, newWeatherForecast);
                logger.LogInformation("{0} have been cached.", cacheKey);
                return JsonConvert.DeserializeObject<WeatherForecast>(newWeatherForecast);
            }
            logger.LogInformation("{0} have been cached, serving from cache.", cacheKey);
            return JsonConvert.DeserializeObject<WeatherForecast>(cachedWeatherForecast);
        }

        public async IAsyncEnumerable<WeatherForecast> GetWeather6Days()
        {
            DateTime[] dates = Enumerable.Range(1, 5).Select(index => DateTime.Now.AddDays(index).Date).ToArray();

            var cacheKeys = dates.Select(d => new { date = d, key = string.Format(cacheKeyTemplate, d.Year + d.Month + d.Day) }).ToArray();
            var cachedWeatherForecasts = cacheKeys.Select(async ck => new { date = ck.date, key = ck.key, value = await this.distributedCache.GetStringAsync(ck.key) });
            await foreach (var cachedWeatherForecast in (await Task.WhenAll(cachedWeatherForecasts)).ToAsyncEnumerable())
            {
                var cacheKey = cachedWeatherForecast.key;
                if (string.IsNullOrWhiteSpace(cachedWeatherForecast.value))
                {
                    logger.LogInformation("{0} havent been fetched.", cacheKey);
                    string request = string.Format(apiRequestTemplate, cachedWeatherForecast.date.ToString(dateTemplate));
                    string newWeatherForecast = await this.httpClient.GetStringAsync(request);
                    await this.distributedCache.SetStringAsync(cachedWeatherForecast.key, newWeatherForecast);
                    logger.LogInformation("{0} have been cached.", cacheKey);
                    yield return JsonConvert.DeserializeObject<WeatherForecast>(newWeatherForecast);
                }
                else
                {
                    logger.LogInformation("{0} have been cached, serving from cache.", cacheKey);
                    yield return JsonConvert.DeserializeObject<WeatherForecast>(cachedWeatherForecast.value);
                }
            }

        }
    }
}
