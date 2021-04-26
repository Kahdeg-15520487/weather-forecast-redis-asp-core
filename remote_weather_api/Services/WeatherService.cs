using Microsoft.Extensions.Logging;

using remote_weather_api;

using remote_weather_api.Services.Contracts;
using remote_weather_api.Services.DTOs;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace remote_weather_api.Services
{
    public class WeatherService : IWeatherService
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherService> logger;

        public WeatherService(ILogger<WeatherService> logger)
        {
            this.logger = logger;
        }

        public WeatherForecast GetWeather(DateTime date)
        {
            DateTime dateWithoutTime = date.Date;
            logger.LogInformation("fetching weather forecast for {0}", dateWithoutTime.ToString("yyyy-MM-dd"));
            Random rng = new Random(dateWithoutTime.Year + dateWithoutTime.Month + dateWithoutTime.Day);

            return new WeatherForecast
            {
                Date = dateWithoutTime,
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            };
        }
    }
}
