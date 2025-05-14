using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using TestAspire.ApiService.DataTransferObjects;

namespace TestAspire.ApiService.Services;

public class ResultsPublisherService(ChannelFactory channelFactory, ILogger<ResultsPublisherService> logger)
    : IDisposable
{
    private readonly IModel _messageChannel = channelFactory.GetCalculationRequestsChannel();

    public void Dispose()
    {
        _messageChannel?.Dispose();
    }


    public void Send(ResultDto results)
    {
        var message = JsonSerializer.Serialize(results);
        var body = Encoding.UTF8.GetBytes(message);
        _messageChannel.BasicPublish(string.Empty, channelFactory.RequestsQueueName,
            body: body);

        logger.LogInformation(
            $"Sent Request {results.Id} for algo {results.Algo.Name} for dataset {results.Dataset.Name} (Id: {results.Dataset.Id})");
    }
}