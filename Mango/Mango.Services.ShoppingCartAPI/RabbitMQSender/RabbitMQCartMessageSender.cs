using RabbitMQ.Client;
using System.Text;
using Newtonsoft.Json;

namespace Mango.Services.ShoppingCartAPI.RabbitMQSender
{
    public class RabbitMQCartMessageSender : IRabbitMQCartMessageSender
    {
        private readonly string _hostName;
        private readonly string _username;
        private readonly string _password;
        private IConnection? _connection; // Make _connection nullable to fix CS8618

        public RabbitMQCartMessageSender()
        {
            this._hostName = "localhost";
            this._username = "guest";
            this._password = "guest";
        }
        public async Task SendMessage(object message, string queueName)
        {
            //var factory = new ConnectionFactory()
            //{
            //    HostName = _hostName,
            //    UserName = _username,
            //    Password = _password
            //};


            if (ConnctionExists())
            {
                this._connection = await factory.CreateConnectionAsync();
                using var channel = await _connection.CreateChannelAsync();
                await channel.QueueDeclareAsync(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

                var json = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(json);
                //await channel.BasicPublishAsync(exchange: "", routingKey: queueName, basicProperties: null, body: body);
                await channel.BasicPublishAsync(exchange: "", routingKey: queueName, body: body);
            }
        }

        private void CreateConnection()
        {
            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = this._hostName,
                    UserName = this._username,
                    Password = this._password
                };
                this._connection = factory.CreateConnection();
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine($"Could not create RabbitMQ connection: {ex.Message}");
            }
        }

        private bool ConnctionExists()
        {
            if (this._connection != null)
            {
                return true;
            }
            CreateConnection();
            return true;
        }
    }
}
