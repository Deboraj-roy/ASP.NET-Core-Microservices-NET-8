using Mango.Services.EmailAPI.Services;
using Microsoft.EntityFrameworkCore.Metadata;
using RabbitMQ.Client;

namespace Mango.Services.EmailAPI.Messaging
{
    public class RabbitMQAuthConsumer : BackgroundService
    {

        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;
        private IConnection? _connection;
        private IModel? _channel;
        private readonly string _queueName;

        public RabbitMQAuthConsumer(IConfiguration configuration, EmailService emailService)
        {
            this._configuration = configuration;
            this._emailService = emailService;
            this._queueName = this._configuration.GetValue<string>("TopicAndQueueNames:RegisterUserQueue");
            //var factory = new ConnectionFactory()
            //{
            //    HostName = "localhost",
            //    UserName = "guest",
            //    Password = "guest",
            //};
            //Old way of creating connection and channel
            //this._connection = factory.CreateConnection();
            //this._channel = _connection.CreateModel();
            //this._channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            this._connection = await factory.CreateConnectionAsync();
            this._channel = await _connection.CreateChannelAsync();
            this._channel.QueueDeclareAsync(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
            };

            this._connection = await factory.CreateConnectionAsync();
            this._channel = await _connection.CreateChannelAsync();
            await this._channel.QueueDeclareAsync(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            // TODO: Implement message consumption logic here
        }

    }
}
