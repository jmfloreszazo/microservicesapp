using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace pairqconsumer
{
    public class Worker : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<Worker> _logger;

        private const string QUEUE_NAME = "pair";

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(66), cancellationToken);

                var queue = ConnectToMQ(cancellationToken);

                queue.QueueDeclare(
                    queue: QUEUE_NAME,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var consumer = new EventingBasicConsumer(queue);
                consumer.Received += (model, ea) =>
                {
                    var pair = Encoding.UTF8.GetString(ea.Body.Span);
                    _logger.LogInformation("Received a new calculated pair number: " + pair);
                };

                queue.BasicConsume(
                    queue: QUEUE_NAME,
                    autoAck: true,
                    consumer: consumer);
            }
            catch (Exception ex)
            {
                _logger.LogError(0, ex, "Failed to start listening to Rabbit MQ");
                throw;
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("*** Worker running at: {time} (@Every10secs)***", DateTimeOffset.Now);
                await Task.Delay(10000, stoppingToken);
            }
        }

        private IModel ConnectToMQ(CancellationToken cancellationToken)
        {
            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = Constants.GetRabbitMQHostName(),
                    Port = int.Parse(Constants.GetRabbitMQPort()),
                    UserName = Constants.GetRabbitMQUser(),
                    Password = Constants.GetRabbitMQPassword(),
                };

                var connection = factory.CreateConnection();
                _logger.LogInformation("Successfully created the connection to RabbitMQ");
                return connection.CreateModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(0, ex, "Failed to start listening to rabbit mq");
                throw;
            }
        }
    }
}