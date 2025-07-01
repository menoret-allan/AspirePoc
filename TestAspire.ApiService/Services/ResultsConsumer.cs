using System.Text.Json;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TestAspire.ApiService.DataTransferObjects;
using TestAspire.ApiService.Entities;

namespace TestAspire.ApiService.Services;

public class ResultsConsumer(
    ChannelFactory channelFactory,
    ILogger<ResultsConsumer> logger,
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

        var queueName = channelFactory.ResultsQueueName;
        logger.LogDebug($"Subscribe to Queue {queueName}");
        _messageChannel.BasicConsume(queueName,
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

    private async void OnConsumerReceived(object? sender, BasicDeliverEventArgs e)
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

        logger.LogTrace(
            $"Received message: {calculationResult.Id}, {calculationResult.Algo}, {calculationResult.ResultJson}");

        logger.LogInformation(
            $"Received result from algo {calculationResult!.Algo.Name} for dataset {calculationResult!.Dataset.Name} (Id: {calculationResult.Dataset.Id})");

        //var resultForDb = autoMapper.Map<Result>(calculationResult);
        using var dbContext = contextFactory.CreateDbContext();

        var target = await dbContext.Results.FindAsync(calculationResult.Id);
        if (target is null)
        {
            logger.LogTrace($"Result with ID {calculationResult.Id} was not found.");
            return;
        }

        target.ResultJson = calculationResult.ResultJson;
        await dbContext.SaveChangesAsync();
        logger.LogTrace($"Result with ID {target.Id} was updated in database");
    }
}