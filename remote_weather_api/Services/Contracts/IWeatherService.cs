using remote_weather_api.Services.DTOs;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace remote_weather_api.Services.Contracts
{
    public interface IWeatherService
    {
        WeatherForecast GetWeather(DateTime date);
    }
}
