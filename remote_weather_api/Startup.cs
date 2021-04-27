using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using remote_weather_api.Services;
using remote_weather_api.Services.Contracts;

using research_redis_key_pattern.core;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace remote_weather_api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddStackExchangeRedisCache(option =>
            {
                option.Configuration = this.Configuration.GetConnectionString("redis"); // config["ConnectionString"]["AzureRedis"]
            });

            services.AddTransient<IWeatherService, WeatherService>();
            services.AddTransient<IDataService, DataService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // custom log response middleware
            app.UseLogResponseMiddlewares();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
