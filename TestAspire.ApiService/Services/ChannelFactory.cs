using RabbitMQ.Client;

namespace TestAspire.ApiService.Services
{
    public class ChannelFactory(
        IConnection messageConnection,
        IConfiguration config)
    {
        const string ConfigKeyName = "RabbitMQ:ResultsQueueName";

        public string ResultsQueueName => config[ConfigKeyName] ?? "Results";

        public IModel GetResultsChannel()
        {
            var messageChannel = messageConnection.CreateModel();
            messageChannel.QueueDeclare(ResultsQueueName, exclusive: false, durable: false, autoDelete: true);
            return messageChannel;
        }
    }
}