using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace remote_weather_api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging((Action<HostBuilderContext, ILoggingBuilder>)((hostingContext, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddConfiguration((IConfiguration)hostingContext.Configuration.GetSection("Logging"));

                    if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development"
                       || Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development.Docker")
                    {
                        logging.AddConsole();
                    }

                    //logging.AddDebug();
                }))
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
