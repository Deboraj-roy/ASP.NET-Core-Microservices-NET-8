using Mango.Services.EmailAPI.Message;
using Mango.Services.EmailAPI.Models.Dto;
using Mango.Services.EmailAPI.Services;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Threading.Channels;

namespace Mango.Services.EmailAPI.Messaging
{
    public class RabbitMQOrderConsumer : BackgroundService, IAsyncDisposable
    {

        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;
        private IConnection? _connection;
        private IChannel? _channel;
        private readonly string? _queueNameApp = "";
        private string? _queueName = "";

        public RabbitMQOrderConsumer(IConfiguration configuration, EmailService emailService)
        {
            this._configuration = configuration;
            this._emailService = emailService;
            this._queueNameApp = this._configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic");
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
            await this._channel.ExchangeDeclareAsync(_queueNameApp, ExchangeType.Fanout);
            var queueDeclareOk = await this._channel.QueueDeclareAsync();
            this._queueName = queueDeclareOk.QueueName;

            _channel.QueueBindAsync(_queueName, _queueNameApp, "");
            //_channel.QueueBindAsync(queue: this._queueName, exchange: _queueNameApp, routingKey: "");

            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new RabbitMQ.Client.Events.AsyncEventingBasicConsumer(this._channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var content = System.Text.Encoding.UTF8.GetString(body);
                RewardsMessage rewardsMessage = JsonConvert.DeserializeObject<RewardsMessage>(content);
                await HandleMessage(rewardsMessage);
                await this._channel.BasicAckAsync(ea.DeliveryTag, false);
            };

            await this._channel.BasicConsumeAsync(queue: _queueName, autoAck: false, consumer: consumer);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task HandleMessage(RewardsMessage rewardsMessage)
        {
            await _emailService.LogOrderPlaced(rewardsMessage);
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

