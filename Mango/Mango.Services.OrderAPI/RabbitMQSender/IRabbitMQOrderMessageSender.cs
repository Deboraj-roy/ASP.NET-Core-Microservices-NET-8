namespace Mango.Services.OrderAPI.RabbitMQSender
{
    public interface IRabbitMQOrderMessageSender
    {
        Task SendMessage(object message, string exchangeName);
    }
}
