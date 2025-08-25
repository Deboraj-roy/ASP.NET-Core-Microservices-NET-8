
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Collections.Concurrent;
using System.Text;

namespace Mango.MessageBus
{
    public class MessageBus : IMessageBus
    {
        //public Task PublishMessage(object message, string topic_queue_Name)
        //{
        //    throw new NotImplementedException();
        //}

        // RabbitMQ
        private string RabbitHostName = "localhost";
        private string RabbitQueueName = "myqueue";
        private string _hostName = "";
        private string _queueName = "";

        // MediatR / In-memory queue
        private static readonly ConcurrentQueue<string> _queue = new();

        private string connectionString = "Endpoint=sb://mangoweb.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=HjoslS58pPHtAULb0tay/jx4Ys0+MO5/R+ASbCcFTG0=";

        //AWS
        private string queueUrl = "";

        public async Task PublishMessage(object message, string topic_queue_Name)
        {
            await using var client = new ServiceBusClient(connectionString);

            ServiceBusSender sender = client.CreateSender(topic_queue_Name);

            var jsonMessage = JsonConvert.SerializeObject(message);
            ServiceBusMessage finalMessage = new ServiceBusMessage(Encoding
                .UTF8.GetBytes(jsonMessage))
            {
                CorrelationId = Guid.NewGuid().ToString(),
            };

            await sender.SendMessageAsync(finalMessage);
            await client.DisposeAsync();
        }

        public async Task PublishRabbitAsync(object message)
        {
            throw new NotImplementedException();
            //var factory = new ConnectionFactory() { HostName = RabbitHostName };
            //using var connection = await factory.CreateConnectionAsync();
            //using var channel = await connection.CreateModelAsync();

            //channel.QueueDeclare(queue: RabbitQueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            //var json = JsonConvert.SerializeObject(message);
            //var body = Encoding.UTF8.GetBytes(json);

            //await channel.BasicPublishAsync(exchange: "", routingKey: RabbitQueueName, mandatory: false, basicProperties: null, body: body);
        }

        //public async Task PublishAsync<T>(T message)
        //{
        //    var factory = new ConnectionFactory { HostName = _hostName };

        //    using var connection = factory.CreateConnectionAsync();
        //    using var channel = connection.CreateModel();

        //    channel.QueueDeclare(_queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

        //    var json = JsonSerializer.Serialize(message);
        //    var body = Encoding.UTF8.GetBytes(json);

        //    channel.BasicPublish(exchange: "", routingKey: _queueName, basicProperties: null, body: body);

        //    await Task.CompletedTask;
        //}

        public void PublishInMemory(object message)
        {
            var json = JsonConvert.SerializeObject(message);
            _queue.Enqueue(json);
        }

        public string? ReceiveInMemory()
        {
            _queue.TryDequeue(out var message);
            return message;
        }

        public async Task PublishAWSAsync(object message, string topicArn, string queueUrl)
        {
            // Publish to SNS
            using var snsClient = new AmazonSimpleNotificationServiceClient();
            await snsClient.PublishAsync(new PublishRequest
            {
                TopicArn = topicArn,
                Message = JsonConvert.SerializeObject(message)
            });

            // Optional: receive from SQS
            using var sqsClient = new AmazonSQSClient();
            var response = await sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
            {
                QueueUrl = queueUrl,
                MaxNumberOfMessages = 10,
                WaitTimeSeconds = 20
            });

            // Process messages if needed
            foreach (var msg in response.Messages)
            {
                // Example: remove from queue after processing
                await sqsClient.DeleteMessageAsync(queueUrl, msg.ReceiptHandle);
            }
        }
    }
}
