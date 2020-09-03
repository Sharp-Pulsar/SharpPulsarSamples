using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Pulsar
{
    public class Program
    {
        public static PulsarConnector PulsarConnector = new PulsarConnector();
        public static void Main(string[] args)
        {
            var dir = AppDomain.CurrentDomain.BaseDirectory;
            File.WriteAllText(@"C:\Users\Technical\source\repos\SharpPulsarSamples\Pulsar\bin\Debug\netcoreapp3.1\dir.log", dir);
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
