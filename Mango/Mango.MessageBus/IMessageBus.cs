using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mango.MessageBus
{
    public interface IMessageBus
    {
        Task PublishMessage(object message, string topic_queue_Name);

        Task PublishRabbitAsync(object message);
        void PublishInMemory(object message);
        string? ReceiveInMemory();
        Task PublishAWSAsync(object message, string topicArn, string queueUrl);
    }
}
