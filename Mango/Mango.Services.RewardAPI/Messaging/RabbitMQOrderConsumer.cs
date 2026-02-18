
using Mango.Services.RewardAPI.Message;
using Mango.Services.RewardAPI.Services;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Mango.Services.RewardAPI.Messaging
{
    public class RabbitMQOrderConsumer : BackgroundService, IAsyncDisposable
    {

        private readonly IConfiguration _configuration;
        private readonly RewardService _rewardService;
        //private readonly EmailService _emailService;
        private IConnection? _connection;
        private IChannel? _channel;
        private readonly string? _queueNameApp = "";
        private string? _queueName = "";

        private const string OrderCreated_RewardsUpdateQueue = "RewardsUpdateQueue";
        private string ExchangeName = "";

        public RabbitMQOrderConsumer(IConfiguration configuration, RewardService rewardService)
        {
            this._configuration = configuration;
            this._rewardService = rewardService;
            this._queueNameApp = this._configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic");
            this.ExchangeName = this._configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic");
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
            //await this._channel.ExchangeDeclareAsync(_queueNameApp, ExchangeType.Fanout);
            await this._channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Direct);
            //var queueDeclareOk = await this._channel.QueueDeclareAsync();
            //this._queueName = queueDeclareOk.QueueName;
            //await _channel.QueueBindAsync(_queueName, _queueNameApp, "");

            await _channel.QueueDeclareAsync(OrderCreated_RewardsUpdateQueue, false, false, false, null);
            await _channel.QueueBindAsync(OrderCreated_RewardsUpdateQueue, ExchangeName, "RewardsUpdate");

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

            //await this._channel.BasicConsumeAsync(queue: _queueName, autoAck: false, consumer: consumer);
            await this._channel.BasicConsumeAsync(queue: OrderCreated_RewardsUpdateQueue, autoAck: false, consumer: consumer);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task HandleMessage(RewardsMessage rewardsMessage)
        {
            await _rewardService.UpdateRewards(rewardsMessage);
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

