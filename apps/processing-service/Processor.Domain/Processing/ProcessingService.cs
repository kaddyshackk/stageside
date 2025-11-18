using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using StageSide.Domain.Models;
using StageSide.Pipeline.Models;

namespace StageSide.Processor.Domain.Processing;

public class ProcessingService(IServiceScopeFactory scopeFactory, ILogger<ProcessingService> logger)
{
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
                using (LogContext.PushProperty("Sku", context.SkuKey))
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