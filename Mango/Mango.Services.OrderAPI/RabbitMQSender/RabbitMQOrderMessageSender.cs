using RabbitMQ.Client;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Mango.Services.OrderAPI.RabbitMQSender
{
    public class RabbitMQOrderMessageSender : IRabbitMQOrderMessageSender
    {
        private readonly string _hostName;
        private readonly string _username;
        private readonly string _password;
        private IConnection? _connection; // Make _connection nullable to fix CS8618
        private const string OrderCreated_RewardsUpdateQueue = "RewardsUpdateQueue";
        private const string OrderCreated_EmailUpdateQueue = "EmailUpdateQueue";


        public RabbitMQOrderMessageSender()
        {
            this._hostName = "localhost";
            this._username = "guest";
            this._password = "guest";
        }
        public async Task SendMessage(object message, string exchangeName)
        {
            //var factory = new ConnectionFactory()
            //{
            //    HostName = _hostName,
            //    UserName = _username,
            //    Password = _password
            //};
            //this._connection = await factory.CreateConnectionAsync();


            if (await ConnectionExists())
            {
                using var channel = await this._connection.CreateChannelAsync();
                //await channel.ExchangeDeclareAsync(exchange: exchangeName, ExchangeType.Fanout, durable: false);
                await channel.ExchangeDeclareAsync(exchange: exchangeName, ExchangeType.Direct, durable: false);

                await channel.QueueDeclareAsync(OrderCreated_EmailUpdateQueue, false, false, false, null);
                await channel.QueueDeclareAsync(OrderCreated_RewardsUpdateQueue, false, false, false, null);

                await channel.QueueBindAsync(OrderCreated_EmailUpdateQueue, exchangeName, "EmailUpdate");
                await channel.QueueBindAsync(OrderCreated_RewardsUpdateQueue, exchangeName, "RewardsUpdate");

                var json = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(json);

                await channel.BasicPublishAsync(exchange: exchangeName, routingKey: "EmailUpdate", body: body);
                await channel.BasicPublishAsync(exchange: exchangeName, routingKey: "RewardsUpdate", body: body);
            }
        }
        private async Task CreateConnection()
        {
            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = this._hostName,
                    UserName = this._username,
                    Password = this._password
                };
                this._connection = await factory.CreateConnectionAsync();
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"Could not create RabbitMQ connection: {ex.Message}");
            }
        }

        private async Task<bool> ConnectionExists()
        {
            if (this._connection != null)
            {
                return true;
            }
            await CreateConnection();
            return this._connection != null;
        }
    }
}
