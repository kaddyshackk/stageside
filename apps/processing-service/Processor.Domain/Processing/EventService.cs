using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StageSide.Domain.Models;
using StageSide.Pipeline.Models;
using StageSide.Processor.Domain.Database;

namespace StageSide.Processor.Domain.Processing
{
    public class EventService(
        IComedyDbContextSession session,
        ILogger<EventService> logger
    )
    {
        public async Task<BatchProcessResult<ProcessedEvent, Event>> ProcessEventsAsync(IEnumerable<ProcessedEvent> processedEvents, CancellationToken ct)
        {
            try
            {
                var events = processedEvents.ToList();
                if (events.Count == 0) return new BatchProcessResult<ProcessedEvent, Event>();

                await session.BeginTransactionAsync(ct);

                var eventSlugs = events.Select(e => e.Slug).Distinct().ToList();
                var existingEvents = await session.Events.Query()
                    .Where(e => eventSlugs.Contains(e.Slug))
                    .ToDictionaryAsync(e => e.Slug, e => e, ct);

                var actSlugs = events.Select(x => x.ComedianSlug!).Distinct().ToList();
                var existingActs = await session.Acts.Query()
                    .Where(a => actSlugs.Contains(a.Slug))
                    .ToDictionaryAsync(a => a.Slug, a => a, ct);

                var venueSlugs = events.Select(e => e.VenueSlug!).Distinct().ToList();
                var existingVenues = await session.Venues.Query()
                    .Where(v => venueSlugs.Contains(v.Slug))
                    .ToDictionaryAsync(v => v.Slug, v => v, ct);

                var newEvents = new List<Event>();
                var newEventActs = new List<EventAct>();
                var updatedEvents = new List<Event>();
                var failedEvents = new List<ProcessedEvent>();

                foreach (var @event in events)
                {
                    if (!existingActs.TryGetValue(@event.ComedianSlug!, out var comedian))
                    {
                        logger.LogWarning("Could not find matching comedian for event.");
                        failedEvents.Add(@event);
                        continue;
                    }

                    if (!existingVenues.TryGetValue(@event.VenueSlug!, out var venue))
                    {
                        logger.LogWarning("Could not find matching venue for event.");
                        failedEvents.Add(@event);
                        continue;
                    }

                    if (existingEvents.TryGetValue(@event.Slug!, out var existingEvent))
                    {
                        var hasChanges = false;

                        if (existingEvent.Title != @event.Title)
                        {
                            existingEvent.Title = @event.Title!;
                            hasChanges = true;
                        }

                        if (existingEvent.StartDateTime != @event.StartDateTime)
                        {
                            existingEvent.StartDateTime = @event.StartDateTime!.Value;
                            hasChanges = true;
                        }

                        if (existingEvent.VenueId != venue.Id)
                        {
                            existingEvent.VenueId = venue.Id;
                            existingEvent.Venue = venue;
                            hasChanges = true;
                        }

                        if (!hasChanges) continue;
                        existingEvent.UpdatedAt = DateTimeOffset.UtcNow;
                        existingEvent.UpdatedBy = "System";
                        updatedEvents.Add(existingEvent);
                    }
                    else if (newEvents.All(ne => ne.Slug != @event.Slug))
                    {
                        var newEvent = new Event
                        {
                            Title = @event.Title!,
                            Slug = @event.Slug!,
                            Status = EventStatus.Scheduled,
                            StartDateTime = @event.StartDateTime!.Value,
                            EndDateTime = @event.EndDateTime,
                            VenueId = venue.Id,
                            Venue = venue,
                            CreatedAt = DateTimeOffset.UtcNow,
                            CreatedBy = "System",
                            UpdatedAt = DateTimeOffset.UtcNow,
                            UpdatedBy = "System"
                        };

                        newEvents.Add(newEvent);

                        newEventActs.Add(new EventAct
                        {
                            ActId = comedian.Id,
                            EventId = newEvent.Id,
                            Act = comedian,
                            Event = newEvent,
                            CreatedAt = DateTimeOffset.UtcNow,
                            CreatedBy = "System",
                            UpdatedAt = DateTimeOffset.UtcNow,
                            UpdatedBy = "System"
                        });
                    }
                }

                if (newEvents.Count != 0)
                    await session.Events.AddRangeAsync(newEvents, ct);
                
                if (updatedEvents.Count != 0)
                    session.Events.UpdateRange(updatedEvents);

                if (newEventActs.Count != 0)
                    await session.EventActs.AddRangeAsync(newEventActs, ct);
                
                await session.SaveChangesAsync(ct);
                await session.CommitTransactionAsync(ct);

                return new BatchProcessResult<ProcessedEvent, Event>
                {
                    Created = newEvents,
                    Updated = updatedEvents,
                    Failed = failedEvents,
                    ProcessedCount = events.Count
                };
            }
            catch (Exception e)
            {
                await session.RollbackTransactionAsync(ct);
                session.Dispose();
                logger.LogError(e, "Batch processing of events failed.");
                throw;
            }
        }
    }
}