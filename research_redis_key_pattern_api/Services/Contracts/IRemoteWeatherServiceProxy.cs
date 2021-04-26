using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace research_redis_key_pattern_api.Services.Contracts
{
    public interface IRemoteWeatherServiceProxy
    {
        Task<WeatherForecast> GetWeather(DateTime date);
        IAsyncEnumerable<WeatherForecast> GetWeather6Days();
    }
}
