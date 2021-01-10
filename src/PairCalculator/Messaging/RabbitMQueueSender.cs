using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;

namespace paircalculator.Messaging
{
    public class RabbitMQueueSender : IMessageQueueSender
    {
        private readonly ILogger<RabbitMQueueSender> _logger;
        private readonly IMQClient _mqClient;

        private IModel _mqChannel;
        private string _queueName;

        private IModel MQChannel
        {
            get
            {
                if(_mqChannel == null || _mqChannel.IsClosed)
                    _mqChannel = _mqClient.CreateChannel();
                return _mqChannel;
            } 
        }

        public RabbitMQueueSender(ILogger<RabbitMQueueSender> logger, IMQClient mqClient)
        {
            _logger = logger;
            _mqClient = mqClient;
        }

        public void Send(string queueName, string message)
        {
            if (string.IsNullOrWhiteSpace(queueName)) return;

            if (string.IsNullOrWhiteSpace(_queueName))
            {
                _logger.LogInformation($"Creating the Rabbit MQ Queue:{queueName} first time");

                MQChannel.QueueDeclare(queue: queueName,
                                            durable: false,
                                            exclusive: false,
                                            autoDelete: false,
                                            arguments: null);
                _queueName = queueName;
            }

            _logger.LogInformation($"Sending msg to mq q:{queueName}");

            var body = Encoding.UTF8.GetBytes(message);

            try
            {
                MQChannel.BasicPublish(exchange: "",
                                            routingKey: queueName,
                                            basicProperties: null,
                                            body: body);
            }
            catch (System.Exception ex)
            {
                ex.ToString();
            }
            _logger.LogInformation("Message sent to the MQ Q successfully");
        }
    }
}
