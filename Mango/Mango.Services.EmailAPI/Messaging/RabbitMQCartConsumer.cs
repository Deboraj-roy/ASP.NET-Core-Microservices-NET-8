using Mango.Services.EmailAPI.Models.Dto;
using Mango.Services.EmailAPI.Services;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Mango.Services.EmailAPI.Messaging
{
    public class RabbitMQCartConsumer : BackgroundService, IAsyncDisposable
    {

        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;
        private IConnection? _connection;
        private IChannel? _channel;
        private readonly string? _queueName = "";

        public RabbitMQCartConsumer(IConfiguration configuration, EmailService emailService)
        {
            this._configuration = configuration;
            this._emailService = emailService;
            this._queueName = this._configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCartQueue");
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
            this._channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);
            await this._channel.QueueDeclareAsync(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null, cancellationToken: stoppingToken);

            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new RabbitMQ.Client.Events.AsyncEventingBasicConsumer(this._channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var content = System.Text.Encoding.UTF8.GetString(body);
                CartDto cartDto = JsonConvert.DeserializeObject<CartDto>(content);
                await HandleMessage(cartDto);
                await this._channel.BasicAckAsync(ea.DeliveryTag, false);
            };

            await this._channel.BasicConsumeAsync(queue: _queueName, autoAck: false, consumer: consumer);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task HandleMessage(CartDto cart)
        {
            await _emailService.EmailCartAndLog(cart);
        }


        public async ValueTask DisposeAsync()
        {
            if (this._channel != null)
            {
                await this._channel.CloseAsync();
            }
            
            if (this._connection != null)
            {
                await this._connection.CloseAsync();
            }

            GC.SuppressFinalize(this);
        }

        public override void Dispose()
        {
            DisposeAsync().AsTask().GetAwaiter().GetResult();
            base.Dispose();
        }
    }
}

