using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using TestAspire.AlgorithmDummy.DataTransferObjects;

namespace TestAspire.AlgorithmDummy;

public class AlgorithmWorker(ILogger<AlgorithmWorker> logger, ChannelFactory channelFactory) : BackgroundService

{
    private readonly IModel _resultsChannel = channelFactory.GetCalculationResultsChannel();
    private readonly IModel _requestsChannel = channelFactory.GetCalculationRequestsChannel();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        SubscribeCalculationRequests();
    }

    private void SubscribeCalculationRequests()
    {
        var consumer = new EventingBasicConsumer(_requestsChannel);
        consumer.Received += OnMessageReceived;
        consumer.Shutdown += OnConsumerShutdown;

        _requestsChannel.BasicConsume(channelFactory.RequestsQueueName,
            true,
            consumer);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _resultsChannel?.Dispose();
        _requestsChannel?.Dispose();

        await base.StopAsync(cancellationToken);
    }

    private void OnConsumerShutdown(object? sender, ShutdownEventArgs shutdownEventArgs)
    {
        logger.LogError(shutdownEventArgs.Exception,
            $"Consumer shutdown event received: {nameof(shutdownEventArgs.Initiator)} = '{shutdownEventArgs.Initiator}', {nameof(shutdownEventArgs.ReplyCode)} = '{shutdownEventArgs.ReplyCode}', {nameof(shutdownEventArgs.ReplyText)} = '{shutdownEventArgs.ReplyText}', {nameof(shutdownEventArgs.ClassId)} = '{shutdownEventArgs.ClassId}', {nameof(shutdownEventArgs.MethodId)} = '{shutdownEventArgs.MethodId}', {nameof(shutdownEventArgs.Cause)} = '{shutdownEventArgs.Cause}'");
    }

    private void OnMessageReceived(object? sender, BasicDeliverEventArgs e)
    {
        logger.LogInformation(
            $"Processing Result at: {DateTime.UtcNow} with messageId: {e.BasicProperties.MessageId}");

        var message = e.Body;

        var request = JsonSerializer.Deserialize<ResultDto>(message.Span);
        if (request is null)
        {
            logger.LogError(
                $"Message with messageId: {e.BasicProperties.MessageId} could not be deserialized", message,
                e.Exchange);
            return;
        }

        logger.LogInformation(
            $"Received result from algo {request!.Algo.Name} for dataset {request!.Dataset.Name} (Id: {request.Dataset.Id})");

        var result = DoHeavyCalculation(request);

        logger.LogInformation(
            $"Sent Request {result.Id} for algo {result.Algo.Name} for dataset {result.Dataset.Name} (Id: {result.Dataset.Id})");
        SendResponse(result);
    }

    private static ResultDto DoHeavyCalculation(ResultDto request)
    {
        request.Algo.Name = "AlgorithmDummy";
        request.Algo.Version = "1.0.0";

        request.ResultJson = request.Dataset.Name + request.Dataset.Image;
        return request;
    }

    private void SendResponse(ResultDto result)
    {
        var message = JsonSerializer.Serialize(result);
        var body = Encoding.UTF8.GetBytes(message);
        _resultsChannel.BasicPublish(string.Empty, channelFactory.ResultsQueueName,
            body: body);
    }
}