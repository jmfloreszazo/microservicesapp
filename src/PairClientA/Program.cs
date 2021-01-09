using microservicesapp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace pairclienta
{
    public class Program
    {
        private static readonly Uri MANUAL_DEBUGTIME_URI = new Uri("https://localhost:5051"); //From launchSettings of the paircalculator

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    var configurationFromHostBuilderContext = hostContext.Configuration;

                    services.AddHostedService<Worker>();

                    services.AddGrpcClient<PairCalculator.PairCalculatorClient>(o =>
                    {
                        //paircalculator will be inject to configuration via Tye
                        o.Address = configurationFromHostBuilderContext.GetServiceUri("paircalculator") ?? MANUAL_DEBUGTIME_URI;
                    });
                });
    }
}
