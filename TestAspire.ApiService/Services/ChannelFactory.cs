using RabbitMQ.Client;

namespace TestAspire.ApiService.Services
{
    public class ChannelFactory(
        IConnection messageConnection,
        IConfiguration config)
    {
        const string ResultsConfigKey = "RabbitMQ:ResultsQueueName";
        const string RequestsConfigKey = "RabbitMQ:RequestsQueueName";
        private const string AlgoInfoConfigKey = "RabbitMQ:AlgoInfoQueueName";

        public string ResultsQueueName => config[ResultsConfigKey] ?? "Results";
        public string RequestsQueueName => config[RequestsConfigKey] ?? "Requests";
        public string AlgoInfoQueueName => config[AlgoInfoConfigKey] ?? "AlgoInfo";

        public IModel GetCalculationResultsChannel()
        {
            var messageChannel = messageConnection.CreateModel();
            messageChannel.QueueDeclare(ResultsQueueName, exclusive: false, durable: false, autoDelete: true);
            return messageChannel;
        }


        public IModel GetCalculationRequestsChannel()
        {
            var messageChannel = messageConnection.CreateModel();
            messageChannel.QueueDeclare(RequestsQueueName, exclusive: false, durable: false, autoDelete: true);
            return messageChannel;
        }

        public IModel GetAlgoInfoChannel()
        {
            var messageChannel = messageConnection.CreateModel();
            messageChannel.QueueDeclare(AlgoInfoQueueName, exclusive: false, durable: false, autoDelete: true);
            return messageChannel;
        }
    }
}