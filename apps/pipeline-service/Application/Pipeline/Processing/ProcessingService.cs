using ComedyPull.Domain.Interfaces.Service;
using ComedyPull.Domain.Models;
using ComedyPull.Domain.Models.Pipeline;
using ComedyPull.Domain.Models.Queue;
using ComedyPull.Domain.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ComedyPull.Application.Pipeline.Processing
{
    public class ProcessingService(
        IQueueClient queueClient,
        IQueueHealthMonitor queueHealthMonitor,
        IBackPressureManager backPressureManager,
        ActService actService,
        VenueService venueService,
        EventService eventService,
        IOptions<ProcessingOptions> options,
        ILogger<ProcessingService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Starting {Service}", nameof(ProcessingService));
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var adaptiveBatchSize = await backPressureManager.CalculateAdaptiveBatchSizeAsync(
                        Queues.Processing,
                        options.Value.MinBatchSize,
                        options.Value.MaxBatchSize);

                    var processingStartTime = DateTime.UtcNow;

                    var batch = await queueClient.DequeueBatchAsync(
                        Queues.Processing,
                        maxCount: adaptiveBatchSize,
                        stoppingToken: stoppingToken);

                    if (batch.Count <= 0) continue;
                    
                    logger.LogInformation("Processing batch of {BatchSize} items (adaptive size: {AdaptiveSize})",
                        batch.Count, adaptiveBatchSize);

                    await ProcessBatchAsync(batch, stoppingToken);

                    var processingTime = DateTime.UtcNow - processingStartTime;
                    await queueHealthMonitor.RecordDequeueAsync(Queues.Processing, batch.Count, processingTime);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing batch in {Service}", nameof(ProcessingService));
                    await queueHealthMonitor.RecordErrorAsync(Queues.Processing);
                }
                finally
                {
                    var delaySeconds = await backPressureManager.CalculateAdaptiveDelayAsync(Queues.Processing,
                        options.Value.DelayIntervalSeconds);
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
                }
            }
            logger.LogInformation("Stopping {Service}", nameof(ProcessingService));
        }
        
        private async Task ProcessBatchAsync(ICollection<PipelineContext> batch, CancellationToken stoppingToken)
        {
            foreach (var context in batch.ToList())
            {
                try
                {
                    // Process Acts
                    var acts = context.ProcessedEntities
                        .Where(e => e.Type == EntityType.Act)
                        .Select(e => e.Data)
                        .Cast<ProcessedAct>();

                    await actService.ProcessActsAsync(acts, stoppingToken);

                    // Process Venues
                    var venues = context.ProcessedEntities
                        .Where(e => e.Type == EntityType.Venue)
                        .Select(e => e.Data)
                        .Cast<ProcessedVenue>();

                    await venueService.ProcessVenuesAsync(venues, stoppingToken);

                    // Process Events
                    var events = context.ProcessedEntities
                        .Where(e => e.Type == EntityType.Event)
                        .Select(e => e.Data)
                        .Cast<ProcessedEvent>();

                    await eventService.ProcessEventsAsync(events, stoppingToken);
                    
                    context.State = ProcessingState.Completed;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing context {ContextId}", context.Id);
                    context.State = ProcessingState.Failed;
                    await queueHealthMonitor.RecordErrorAsync(Queues.Processing);
                }
            }
        }
    }
}