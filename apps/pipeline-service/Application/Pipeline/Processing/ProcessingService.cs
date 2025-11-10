using System.Text.Json;
using ComedyPull.Domain.Models;
using ComedyPull.Domain.Pipeline;
using ComedyPull.Domain.Pipeline.Interfaces;
using ComedyPull.Domain.Pipeline.Models;
using ComedyPull.Domain.Queue.Interfaces;
using ComedyPull.Domain.Queue.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog.Context;

namespace ComedyPull.Application.Pipeline.Processing
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
                logger.LogInformation("Started {Service}", nameof(ProcessingService));
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
                        logger.LogError(ex, "Error processing batch in {Service}", nameof(ProcessingService));
                    }
                    finally
                    {
                        var delaySeconds = await backPressureManager.CalculateAdaptiveDelayAsync(Queues.Processing,
                            options.Value.DelayIntervalSeconds);
                        await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
                    }
                }
                logger.LogInformation("Stopped {Service}", nameof(ProcessingService));
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
                using (LogContext.PushProperty("ExecutionId", context.JobExecutionId))
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