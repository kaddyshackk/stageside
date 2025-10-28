using System.Text.Json;
using ComedyPull.Application.Exceptions;
using ComedyPull.Domain.Interfaces.Service;
using ComedyPull.Domain.Models;
using ComedyPull.Domain.Models.Pipeline;
using ComedyPull.Domain.Models.Queue;
using ComedyPull.Domain.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog.Context;

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
            while (!stoppingToken.IsCancellationRequested)
            {
                var batch = await queueClient.DequeueBatchAsync(Queues.Processing, 20);
                var records = batch.ToList();

                var groups = records.GroupBy(r => r.EntityType);
                foreach (var group in groups)
                {
                    switch (group.Key)
                    {
                        case EntityType.Act:
                            await ProcessActsAsync(group.AsEnumerable());
                            break;
                        case EntityType.Event:
                            await ProcessEventsAsync(group.AsEnumerable());
                            break;
                        case EntityType.Venue:
                            await ProcessVenuesAsync(group.AsEnumerable());
                            break;
                        default:
                            throw new InvalidEntityTypeException("Batch-Id", group.Key);
                    }
                }
            }
        }
        
        private async Task ProcessActsAsync(IEnumerable<SilverRecord> silverRecords)
        {
            var records = silverRecords.ToList();
            var processed = records
                .Select(r =>
                {
                    try
                    {
                        return JsonSerializer.Deserialize<ProcessedAct>(r.Data);
                    }
                    catch (Exception ex)
                    {
                        using (LogContext.PushProperty("RecordId", r.Id))
                        {
                            logger.LogError(ex, "Failed to deserialize ProcessedAct");
                            r.State = ProcessingState.Failed;
                            return null;
                        }
                    }
                })
                .Where(x => x != null);

            var result = await actService.ProcessActsAsync(processed!);
            
            // TODO: Handle result
        }
        
        private async Task ProcessVenuesAsync(IEnumerable<SilverRecord> silverRecords)
        {
            var records = silverRecords.ToList();
            var processed = records
                .Select(r =>
                {
                    try
                    {
                        return JsonSerializer.Deserialize<ProcessedVenue>(r.Data);
                    }
                    catch (Exception ex)
                    {
                        using (LogContext.PushProperty("RecordId", r.Id))
                        {
                            logger.LogError(ex, "Failed to deserialize ProcessedVenue");
                            r.State = ProcessingState.Failed;
                            return null;
                        }
                    }
                })
                .Where(x => x != null);

            var result = await venueService.ProcessVenuesAsync(processed!);
            
            // TODO: Handle result
        }
        
        private async Task ProcessEventsAsync(IEnumerable<SilverRecord> silverRecords)
        {
            var records = silverRecords.ToList();

            var processed = records
                .Select(r =>
                {
                    try
                    {
                        return JsonSerializer.Deserialize<ProcessedEvent>(r.Data);
                    }
                    catch (Exception ex)
                    {
                        using (LogContext.PushProperty("RecordId", r.Id))
                        {
                            logger.LogError(ex, "Failed to deserialize ProcessedEvent");
                            r.State = ProcessingState.Failed;
                            return null;
                        }
                    }
                })
                .Where(x => x != null);

            var result = await eventService.ProcessEventsAsync(processed!);
            
            // TODO: Handle result
        }
    }
}