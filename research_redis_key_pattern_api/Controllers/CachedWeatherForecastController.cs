using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

using research_redis_key_pattern_api.Services.Contracts;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace research_redis_key_pattern_api.Controllers
{
    /*
     * get weather from a service
     * 
     * 
     * 
     * 
     */
    [ApiController]
    [Route("api/weather")]
    public class CachedWeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly IRemoteWeatherServiceProxy remoteWeatherServiceProxy;
        private readonly ILogger<CachedWeatherForecastController> _logger;

        public CachedWeatherForecastController(IRemoteWeatherServiceProxy remoteWeatherServiceProxy, ILogger<CachedWeatherForecastController> logger)
        {
            this.remoteWeatherServiceProxy = remoteWeatherServiceProxy;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            return await this.remoteWeatherServiceProxy.GetWeather6Days().ToListAsync();
        }

        [HttpGet("{date}")]
        public async Task<WeatherForecast> Get(DateTime date)
        {
            return await this.remoteWeatherServiceProxy.GetWeather(date);
        }
    }
}
