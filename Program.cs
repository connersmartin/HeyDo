using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HeyDo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, logging) =>
            {
                logging.AddJsonFile("appsettings.json",false);
            })
            .ConfigureLogging((context,logging) => 
            {
                logging.ClearProviders();
                //logging.AddFilter("Microsoft", LogLevel.Warning);
                //logging.AddFilter("Hangfire", LogLevel.Warning);
                logging.AddConfiguration(context.Configuration.GetSection("Logging"));
                logging.AddConsole();
                
                logging.AddDebug();
                logging.AddEventSourceLogger();
            })
                .UseStartup<Startup>()
            .UseKestrel();
    }
}
