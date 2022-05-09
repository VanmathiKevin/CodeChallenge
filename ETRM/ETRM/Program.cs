using ETRM.Controller;
using Microsoft.Extensions.Configuration;
using System;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
using System.Configuration;

namespace ETRM
{
    internal class Program
    {
        private static IConfigurationRoot Configuration { get; set; }
        static void Main(string[] args)
        {
            IConfigurationBuilder configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json");
            Configuration = configuration.Build();

            var filePath = Configuration.GetSection("XML:InputFileName").Value.ToString();
            Console.WriteLine(filePath);

            FileWatcher fileWatcher = new FileWatcher();
            fileWatcher.MonitorInputFile(filePath);

            //Global Exception handling, logging and test case can be done to make
            //the code producion ready, but unable to implement due to time-constraint
        }
    }
}
