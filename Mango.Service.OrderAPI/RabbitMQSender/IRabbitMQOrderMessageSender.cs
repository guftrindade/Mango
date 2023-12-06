namespace Mango.Service.OrderAPI.RabbitMQSender;

public interface IRabbitMQOrderMessageSender
{
    void SendMessage(object message, string exchangeName);
}
