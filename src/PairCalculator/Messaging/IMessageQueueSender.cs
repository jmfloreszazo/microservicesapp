namespace paircalculator.Messaging
{
    public interface IMessageQueueSender
    {
        public void Send(string queueName, string message);
    }
}
