using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using remote_weather_api.Services.Contracts;
using remote_weather_api.Services.DTOs;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace remote_weather_api.Controllers
{
    [ApiController]
    [Route("api/weather")]
    public class WeatherForecastController : ControllerBase
    {

        private readonly IWeatherService weatherService;
        private readonly ILogger<WeatherForecastController> logger;

        public WeatherForecastController(IWeatherService weatherService, ILogger<WeatherForecastController> logger)
        {
            this.weatherService = weatherService;
            this.logger = logger;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> GetSevenDays()
        {
            return Enumerable.Range(1, 5).Select(index => weatherService.GetWeather(DateTime.Now.AddDays(index))).ToArray();
        }

        [HttpGet("{date}")]
        public WeatherForecast Get(DateTime date)
        {
            return weatherService.GetWeather(date);
        }
    }
}
