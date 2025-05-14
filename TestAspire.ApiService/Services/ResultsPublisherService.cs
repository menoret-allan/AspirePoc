using System.Text;
using System.Text.Json;
using AutoMapper;
using RabbitMQ.Client;
using TestAspire.ApiService.DataTransferObjects;
using TestAspire.ApiService.Entities;

namespace TestAspire.ApiService.Services;

public class ResultsPublisherService(
    ChannelFactory channelFactory,
    ILogger<ResultsPublisherService> logger)
    : IDisposable
{
    private readonly IModel _messageChannel = channelFactory.GetCalculationRequestsChannel();

    public void Dispose()
    {
        _messageChannel?.Dispose();
    }


    public void Send(ResultDto result)
    {
        var message = JsonSerializer.Serialize(result);
        var body = Encoding.UTF8.GetBytes(message);
        _messageChannel.BasicPublish(string.Empty, channelFactory.RequestsQueueName,
            body: body);

        logger.LogInformation(
            $"Sent Request {result.Id} for algo {result.Algo.Name} for dataset {result.Dataset.Name} (Id: {result.Dataset.Id})");
    }
}