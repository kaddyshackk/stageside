using ComedyPull.Domain.Core.Acts;
using ComedyPull.Domain.Core.Events.Interfaces;
using ComedyPull.Domain.Core.Venues.Interfaces;
using ComedyPull.Domain.Pipeline;
using Microsoft.Extensions.Logging;

namespace ComedyPull.Domain.Core.Events.Services
{
    public class EventService(
        IEventRepository eventRepository,
        IEventActRepository eventActRepository,
        IActRepository actRepository,
        IVenueRepository venueRepository,
        ILogger<EventService> logger
    )
    {
        public async Task<BatchProcessResult<ProcessedEvent, Event>> ProcessEventsAsync(IEnumerable<ProcessedEvent> processedEvents, CancellationToken stoppingToken)
        {
            var events = processedEvents.ToList();
            var eventSlugs = events.Select(e => e.Slug).Distinct().ToList();
            var existingEvents = await eventRepository.GetEventsBySlugAsync(eventSlugs!, stoppingToken);
            var existingEventsBySlug = existingEvents.ToDictionary(e => e.Slug, e => e);
            
            var comedianSlugs = events.Select(x => x.ComedianSlug!).Distinct().ToList();
            var venueSlugs = events.Select(e => e.VenueSlug!).Distinct().ToList();

            var acts = await actRepository.GetActsBySlugAsync(comedianSlugs, stoppingToken);
            var actsBySlug = acts.ToDictionary(c => c.Slug, c => c);

            var venues = await venueRepository.GetVenuesBySlugAsync(venueSlugs, stoppingToken);
            var venuesBySlug = venues.ToDictionary(v => v.Slug, v => v);

            var newEvents = new List<Event>();
            var newEventActs = new List<EventAct>();
            var updatedEvents = new List<Event>();
            var failedEvents = new List<ProcessedEvent>();

            foreach (var @event in events)
            {
                if (!actsBySlug.TryGetValue(@event.ComedianSlug!, out var comedian))
                {
                    logger.LogWarning("Could not find matching comedian for event.");
                    failedEvents.Add(@event);
                    continue;
                }

                if (!venuesBySlug.TryGetValue(@event.VenueSlug!, out var venue))
                {
                    logger.LogWarning("Could not find matching venue for event.");
                    failedEvents.Add(@event);
                    continue;
                }

                if (existingEventsBySlug.TryGetValue(@event.Slug!, out var existingEvent))
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
            {
                await eventRepository.BulkCreateEventsAsync(newEvents, stoppingToken);
                logger.LogInformation("Created {Count} new events", newEvents.Count);
            }

            if (updatedEvents.Count != 0)
            {
                await eventRepository.SaveChangesAsync(stoppingToken);
                logger.LogInformation("Updated {Count} existing events", updatedEvents.Count);
            }

            if (newEventActs.Count != 0)
            {
                await eventActRepository.BulkCreateEventActsAsync(newEventActs, stoppingToken);
                logger.LogInformation("Created {Count} comedian-event relationships", newEventActs.Count);
            }

            return new BatchProcessResult<ProcessedEvent, Event>
            {
                Created = newEvents,
                Updated = updatedEvents,
                Failed = failedEvents,
                ProcessedCount = events.Count
            };
        }
    }
}