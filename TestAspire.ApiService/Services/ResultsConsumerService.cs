using System.Text.Json;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TestAspire.ApiService.DataTransferObjects;
using TestAspire.ApiService.Entities;

namespace TestAspire.ApiService.Services;

public class ResultsConsumerService(
    ChannelFactory channelFactory,
    ILogger<ResultsConsumerService> logger,
    IDbContextFactory<MyDbContext> contextFactory,
    IMapper autoMapper)
    : BackgroundService
{
    private readonly IModel _messageChannel = channelFactory.GetCalculationResultsChannel();

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_messageChannel);
        consumer.Received += OnConsumerReceived;
        consumer.Shutdown += OnConsumerShutdown;

        _messageChannel.BasicConsume(channelFactory.ResultsQueueName,
            true,
            consumer);

        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _messageChannel?.Dispose();

        await base.StopAsync(cancellationToken);
    }

    private void OnConsumerShutdown(object? sender, ShutdownEventArgs shutdownEventArgs)
    {
        logger.LogError(shutdownEventArgs.Exception,
            $"Consumer shutdown event received: {nameof(shutdownEventArgs.Initiator)} = '{shutdownEventArgs.Initiator}', {nameof(shutdownEventArgs.ReplyCode)} = '{shutdownEventArgs.ReplyCode}', {nameof(shutdownEventArgs.ReplyText)} = '{shutdownEventArgs.ReplyText}', {nameof(shutdownEventArgs.ClassId)} = '{shutdownEventArgs.ClassId}', {nameof(shutdownEventArgs.MethodId)} = '{shutdownEventArgs.MethodId}', {nameof(shutdownEventArgs.Cause)} = '{shutdownEventArgs.Cause}'");
    }

    private void OnConsumerReceived(object? sender, BasicDeliverEventArgs e)
    {
        logger.LogInformation(
            $"Processing Result at: {DateTime.UtcNow} with messageId: {e.BasicProperties.MessageId}");

        var message = e.Body;

        var calculationResult = JsonSerializer.Deserialize<ResultDto>(message.Span);
        if (calculationResult is null)
        {
            logger.LogError(
                $"Message with messageId: {e.BasicProperties.MessageId} could not be deserialized", message,
                e.Exchange);
            return;
        }

        logger.LogInformation(
            $"Received result from algo {calculationResult!.Algo.Name} for dataset {calculationResult!.Dataset.Name} (Id: {calculationResult.Dataset.Id})");


        var resultForDb = autoMapper.Map<Result>(calculationResult);
        using var dbContext = contextFactory.CreateDbContext();
        dbContext.Update(resultForDb);
        dbContext.SaveChanges();
    }
}