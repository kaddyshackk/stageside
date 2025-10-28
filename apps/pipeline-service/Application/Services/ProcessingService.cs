using ComedyPull.Domain.Interfaces.Service;
using ComedyPull.Domain.Models;
using ComedyPull.Domain.Models.Pipeline;
using ComedyPull.Domain.Models.Queue;
using ComedyPull.Domain.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ComedyPull.Application.Services
{
    public class ProcessingService(
        IQueueClient queueClient,
        ActService actService,
        VenueService venueService,
        EventService eventService,
        ILogger<ProcessingService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Starting {Service}", nameof(ProcessingService));
            
            while (!stoppingToken.IsCancellationRequested)
            {
                var batch = await queueClient.DequeueBatchAsync(Queues.Processing, 20);
                foreach (var context in batch.ToList())
                {
                    // Process Acts
                    var acts = context.ProcessedEntities
                        .Where(e => e.Type == EntityType.Act)
                        .Select(e => e.Data)
                        .Cast<ProcessedAct>();
                    
                    await actService.ProcessActsAsync(acts);
                    
                    // Process Venues
                    var venues = context.ProcessedEntities
                        .Where(e => e.Type == EntityType.Venue)
                        .Select(e => e.Data)
                        .Cast<ProcessedVenue>();

                    await venueService.ProcessVenuesAsync(venues);
                    
                    // Process Events
                    var events = context.ProcessedEntities
                        .Where(e => e.Type == EntityType.Venue)
                        .Select(e => e.Data)
                        .Cast<ProcessedEvent>();

                    await eventService.ProcessEventsAsync(events);
                }
            }
            logger.LogInformation("Stopping {Service}", nameof(ProcessingService));
        }
    }
}