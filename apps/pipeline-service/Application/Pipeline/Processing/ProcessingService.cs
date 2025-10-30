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
                var batch = await queueClient.DequeueBatchAsync(
                    Queues.Processing,
                    maxCount: options.Value.BatchMaxSize,
                    maxWait: TimeSpan.FromSeconds(options.Value.BatchMaxWaitSeconds),
                    pollingWait: TimeSpan.FromSeconds(options.Value.PollingWaitSeconds),
                    cancellationToken: stoppingToken);

                if (batch.Count > 0)
                {
                    foreach (var context in batch.ToList())
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
                            .Where(e => e.Type == EntityType.Venue)
                            .Select(e => e.Data)
                            .Cast<ProcessedEvent>();

                        await eventService.ProcessEventsAsync(events, stoppingToken);
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(options.Value.BatchDelaySeconds), stoppingToken);
            }
            logger.LogInformation("Stopping {Service}", nameof(ProcessingService));
        }
    }
}