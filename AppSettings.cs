using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace HeyDo
{
    static class AppSettings
    {
        public static IConfiguration AppSetting { get; }
        static AppSettings()
        {
            AppSetting = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("heydosettings.json")
                    .Build();
        }
    }
}
