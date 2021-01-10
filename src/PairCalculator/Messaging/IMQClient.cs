using RabbitMQ.Client;

namespace paircalculator.Messaging
{
    public interface IMQClient
    {
        IModel CreateChannel();
    }
}
