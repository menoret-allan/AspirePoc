using System.Text.Json;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TestAspire.ApiService.DataTransferObjects;
using TestAspire.ApiService.Entities;

namespace TestAspire.ApiService.Services
{
    public class AlgoInfoConsumer(
        ChannelFactory channelFactory,
        ILogger<ResultsConsumer> logger,
        IDbContextFactory<MyDbContext> contextFactory,
        IMapper autoMapper) : BackgroundService
    {
        private readonly IModel _messageChannel = channelFactory.GetCalculationResultsChannel();


        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_messageChannel);
            consumer.Received += OnConsumerReceived;
            consumer.Shutdown += OnConsumerShutdown;

            var queueName = channelFactory.AlgoInfoQueueName;
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


        private void OnConsumerReceived(object? sender, BasicDeliverEventArgs e)
        {
            logger.LogInformation(
                $"Processing Algo Info at: {DateTime.UtcNow} with messageId: {e.BasicProperties.MessageId}");

            var message = e.Body;

            var algoInfo = JsonSerializer.Deserialize<AlgoDto>(message.Span);
            if (algoInfo is null)
            {
                logger.LogError(
                    $"Message with messageId: {e.BasicProperties.MessageId} could not be deserialized", message,
                    e.Exchange);
                return;
            }

            logger.LogTrace($"Received message", algoInfo);

            logger.LogInformation(
                $"Received result from algo {algoInfo.Name} for dataset {algoInfo.Name} (Id: {algoInfo.Id})");

            var algoForDb = autoMapper.Map<Algo>(algoInfo);
            using var dbContext = contextFactory.CreateDbContext();
            dbContext.Algos.Update(algoForDb);
            dbContext.SaveChanges();
            logger.LogTrace($"Algo with ID {algoForDb.Id} was updated in database");
        }
    }
}