using System;
using System.Threading;
using System.Threading.Tasks;
using microservicesapp;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace pairclienta
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly PairCalculator.PairCalculatorClient _pairClient;

        public Worker(ILogger<Worker> logger, PairCalculator.PairCalculatorClient pairClient)
        {
            _logger = logger;
            _pairClient = pairClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //Giving time for all other services/dependencies to be warmed up (ex: RabbitMQ takes time to boot up)
            await Task.Delay(TimeSpan.FromSeconds(33), stoppingToken);
            _logger.LogInformation("Starting to send Pair number requests.......");

            long input = 3;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var response = await _pairClient.IsItPairAsync(new PairRequest { Number = input });
                    _logger.LogInformation($"Is {input} a Pair number? Service tells us: {response.IsPair}\r");
                }
                catch(Exception ex)
                {
                    if (stoppingToken.IsCancellationRequested) return;
                    _logger.LogError(-1, ex, "Error occurred while calling IsItPairAsync() but will continue..");
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); //just adding some delay in case of error..
                }

                input++;

                if (stoppingToken.IsCancellationRequested) break;

                await Task.Delay(TimeSpan.FromMilliseconds(10), stoppingToken);
            }
        }
    }
}
