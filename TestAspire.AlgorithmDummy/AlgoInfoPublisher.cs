using System.Reflection;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using TestAspire.AlgorithmDummy.DataTransferObjects;

namespace TestAspire.AlgorithmDummy;

/// <summary>
///     This class sends a notification on RabbitMQ when the service is started and when it is stopped
/// </summary>
public class AlgoInfoPublisher(ChannelFactory channelFactory, ILogger<AlgoInfoPublisher> logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        SendAlgoInfo(true);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // this is not called, see https://github.com/dotnet/aspire/issues/6885
        SendAlgoInfo(false);
        return Task.CompletedTask;
    }

    private void SendAlgoInfo(bool isAlgoAvailable)
    {
        var channel = channelFactory.GetAlgoInfoChannel();
        var algoInfo = new AlgoDto
        {
            Name = Assembly.GetExecutingAssembly().GetName().Name ?? "AlgorithmDummy",
            Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString()
                      ?? new Version(1, 0, 0).ToString(),
            IsAlive = isAlgoAvailable
        };
        var message = JsonSerializer.Serialize(algoInfo);
        var queueName = channelFactory.AlgoInfoQueueName;

        logger.LogTrace($"Message will be send on Queue {queueName}", message);
        var body = Encoding.UTF8.GetBytes(message);
        channel.BasicPublish(string.Empty, queueName, body: body);
    }
}