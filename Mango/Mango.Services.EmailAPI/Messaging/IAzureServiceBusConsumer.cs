namespace Mango.Services.EmailAPI.Messaging
{
    public class IAzureServiceBusConsumer
    {
        Task Start();
        Task Stop();
    }
}
