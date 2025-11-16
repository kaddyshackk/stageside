using System.Text.Json;
using StageSide.Pipeline.Domain.Pipeline;
using StageSide.Pipeline.Domain.Pipeline.Interfaces;
using StageSide.Pipeline.Domain.Pipeline.Models;
using StageSide.Pipeline.Domain.Queue.Interfaces;
using Microsoft.Extensions.Options;
using Serilog.Context;
using StageSide.Domain.Models;
using StageSide.Pipeline.Domain.Queue;
using StageSide.Pipeline.Service.Pipeline.Options;

namespace StageSide.Pipeline.Service.Pipeline
{
    public class ProcessingService(
        IQueueClient queueClient,
        IBackPressureManager backPressureManager,
        IServiceScopeFactory scopeFactory,
        IOptions<ProcessingOptions> options,
        ILogger<ProcessingService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (LogContext.PushProperty("ServiceName", nameof(ProcessingService)))
            {
                logger.LogInformation("Started {Pipeline.Service}", nameof(ProcessingService));
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var adaptiveBatchSize = await backPressureManager.CalculateAdaptiveBatchSizeAsync(
                            Queues.Processing,
                            options.Value.MinBatchSize,
                            options.Value.MaxBatchSize);

                        var batch = await queueClient.DequeueBatchAsync(
                            Queues.Processing,
                            maxCount: adaptiveBatchSize,
                            stoppingToken: stoppingToken);

                        if (batch.Count <= 0) continue;
                    
                        logger.LogInformation("Processing batch of {BatchSize} items (adaptive size: {AdaptiveSize})",
                            batch.Count, adaptiveBatchSize);

                        await ProcessBatchAsync(batch, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error processing batch in {Pipeline.Service}", nameof(ProcessingService));
                    }
                    finally
                    {
                        var delaySeconds = await backPressureManager.CalculateAdaptiveDelayAsync(Queues.Processing,
                            options.Value.DelayIntervalSeconds);
                        await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
                    }
                }
                logger.LogInformation("Stopped {Pipeline.Service}", nameof(ProcessingService));
            }
        }
        
        private async Task ProcessBatchAsync(ICollection<PipelineContext> batch, CancellationToken stoppingToken)
        {
            using var scope = scopeFactory.CreateScope();
            var actService = scope.ServiceProvider.GetRequiredService<ActService>();
            var venueService = scope.ServiceProvider.GetRequiredService<VenueService>();
            var eventService = scope.ServiceProvider.GetRequiredService<EventService>();
            foreach (var context in batch.ToList())
            {
                using (LogContext.PushProperty("ContextId", context.Id))
                using (LogContext.PushProperty("ExecutionId", context.JobId))
                using (LogContext.PushProperty("Sku", context.Sku))
                using (LogContext.PushProperty("CollectionUrl", context.Metadata.CollectionUrl))
                using (LogContext.PushProperty("Tags", context.Metadata.Tags))
                {
                    try
                    {
                        // Process Acts
                        var acts = context.ProcessedEntities
                            .Where(e => e.Type == EntityType.Act)
                            .Select(e => DeserializeEntity<ProcessedAct>(e.Data))
                            .Where(a => a != null)
                            .Cast<ProcessedAct>();

                        await actService.ProcessActsAsync(acts, stoppingToken);

                        // Process Venues
                        var venues = context.ProcessedEntities
                            .Where(e => e.Type == EntityType.Venue)
                            .Select(e => DeserializeEntity<ProcessedVenue>(e.Data))
                            .Where(v => v != null)
                            .Cast<ProcessedVenue>();

                        await venueService.ProcessVenuesAsync(venues, stoppingToken);

                        // Process Events
                        var events = context.ProcessedEntities
                            .Where(e => e.Type == EntityType.Event)
                            .Select(e => DeserializeEntity<ProcessedEvent>(e.Data))
                            .Where(ev => ev != null)
                            .Cast<ProcessedEvent>();

                        await eventService.ProcessEventsAsync(events, stoppingToken);
                    
                        context.State = ProcessingState.Completed;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error processing context {ContextId}", context.Id);
                        context.State = ProcessingState.Failed;
                    }
                }
            }
        }
        
        private static T? DeserializeEntity<T>(object data) where T : class
        {
            if (data is JsonElement jsonElement)
            {
                return JsonSerializer.Deserialize<T>(jsonElement.GetRawText());
            }
            if (data is T directType)
            {
                return directType;
            }
            return null;
        }
    }
}