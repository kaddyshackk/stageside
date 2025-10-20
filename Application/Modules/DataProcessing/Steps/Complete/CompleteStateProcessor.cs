using ComedyPull.Application.Events;
using ComedyPull.Application.Modules.DataProcessing.Exceptions;
using ComedyPull.Application.Modules.DataProcessing.Interfaces;
using ComedyPull.Application.Modules.DataProcessing.Steps.Complete.Interfaces;
using ComedyPull.Application.Modules.DataProcessing.Steps.Interfaces;
using ComedyPull.Domain.Enums;
using ComedyPull.Domain.Modules.Common;
using ComedyPull.Domain.Modules.DataProcessing;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ComedyPull.Application.Modules.DataProcessing.Steps.Complete
{
    public class CompleteStateProcessor(
        IBatchRepository batchRepository,
        ICompleteStateRepository repository,
        IMediator mediator,
        ILogger<CompleteStateProcessor> logger) : IStateProcessor
    {
        public ProcessingState FromState => ProcessingState.Transformed;
        public ProcessingState ToState => ProcessingState.Completed;

        public async Task ProcessBatchAsync(Guid batchId, CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting {Stage} processing for batch {BatchId}", ToState, batchId);
            try
            {
                var batch = await batchRepository.GetBatchById(batchId, cancellationToken);
                if (batch.State != FromState)
                {
                    throw new InvalidBatchStateException(batchId.ToString(), FromState, batch.State);
                }

                var records = await repository.GetSilverRecordsByBatchId(batchId, cancellationToken);

                logger.LogInformation("Processing {Count} records of type {SourceType} in batch {BatchId}",
                    records.Count(), batch.SourceType, batchId);

                var groups = records.GroupBy(r => r.EntityType);
                foreach (var group in groups)
                {
                    switch (group.Key)
                    {
                        case EntityType.Act:
                            await ProcessActsAsync(group.AsEnumerable(), cancellationToken);
                            break;
                        case EntityType.Event:
                            await ProcessEventsAsync(group.AsEnumerable(), cancellationToken);
                            break;
                        case EntityType.Venue:
                            await ProcessVenuesAsync(group.AsEnumerable(), cancellationToken);
                            break;
                        default:
                            throw new InvalidEntityTypeException(batchId.ToString(), group.Key);
                    }
                }

                // Update batch State & Status
                await batchRepository.UpdateBatchStateById(batchId, ToState, cancellationToken);

                // Signal Event
                await mediator.Publish(new StateCompletedEvent(batchId), cancellationToken);

                logger.LogInformation("Completed {Stage} processing for batch {BatchId}", ToState, batchId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed {Stage} processing for batch {BatchId}", ToState, batchId);
                await batchRepository.UpdateBatchStateById(batchId, ProcessingState.Failed, cancellationToken);
                throw;
            }
        }

        private async Task ProcessActsAsync(IEnumerable<SilverRecord> silverRecords,
            CancellationToken cancellationToken)
        {
            var records = silverRecords.ToList();
            logger.LogInformation("Processing {Count} Act (Comedian) records", records.Count);

            // 1. Parse all ProcessedAct data
            var processedActs = records
                .Select(r =>
                {
                    try
                    {
                        var act = System.Text.Json.JsonSerializer.Deserialize<Domain.Modules.DataProcessing.ProcessedAct>(r.Data);
                        return new { SilverRecord = r, ProcessedAct = act };
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to deserialize ProcessedAct for SilverRecord {RecordId}", r.Id);
                        r.Status = ProcessingStatus.Failed;
                        return null;
                    }
                })
                .Where(x => x?.ProcessedAct != null && !string.IsNullOrWhiteSpace(x.ProcessedAct.Slug))
                .ToList();

            if (processedActs.Count == 0)
            {
                logger.LogWarning("No valid Act records to process");
                return;
            }

            // 2. Batch query for existing comedians by slug
            var slugs = processedActs.Select(x => x.ProcessedAct!.Slug!).Distinct().ToList();
            var existingComedians = await repository.GetComediansBySlugAsync(slugs, cancellationToken);
            var existingComediansBySlug = existingComedians.ToDictionary(c => c.Slug, c => c);

            // 3. Identify new vs existing
            var newComedians = new List<Comedian>();
            var updatedComedians = new List<Comedian>();

            foreach (var item in processedActs)
            {
                var act = item.ProcessedAct!;

                if (existingComediansBySlug.TryGetValue(act.Slug!, out var existingComedian))
                {
                    // Update existing comedian if data changed
                    var hasChanges = false;

                    if (existingComedian.Name != act.Name)
                    {
                        existingComedian.Name = act.Name!;
                        hasChanges = true;
                    }

                    if (!string.IsNullOrWhiteSpace(act.Bio) && existingComedian.Bio != act.Bio)
                    {
                        existingComedian.Bio = act.Bio;
                        hasChanges = true;
                    }

                    if (hasChanges)
                    {
                        existingComedian.UpdatedAt = DateTimeOffset.UtcNow;
                        existingComedian.UpdatedBy = "System";
                        updatedComedians.Add(existingComedian);
                    }

                    item.SilverRecord.Status = ProcessingStatus.Completed;
                }
                else if (newComedians.All(nc => nc.Slug != act.Slug))
                {
                    // Create new comedian (check we haven't already added in this batch)
                    var comedian = new Comedian
                    {
                        Name = act.Name!,
                        Slug = act.Slug!,
                        Bio = act.Bio ?? string.Empty,
                        Source = DataSource.Punchup, // TODO: Get from batch
                        IngestedAt = DateTimeOffset.UtcNow,
                        CreatedAt = DateTimeOffset.UtcNow,
                        CreatedBy = "System",
                        UpdatedAt = DateTimeOffset.UtcNow,
                        UpdatedBy = "System"
                    };

                    newComedians.Add(comedian);
                    item.SilverRecord.Status = ProcessingStatus.Completed;
                }
            }

            // 4. Bulk insert new comedians
            if (newComedians.Count != 0)
            {
                await repository.AddComediansAsync(newComedians, cancellationToken);
                logger.LogInformation("Created {Count} new comedians", newComedians.Count);
            }

            // 5. Bulk update existing comedians
            if (updatedComedians.Count != 0)
            {
                await repository.UpdateComediansAsync(updatedComedians, cancellationToken);
                logger.LogInformation("Updated {Count} existing comedians", updatedComedians.Count);
            }

            // 6. Update SilverRecord statuses
            await repository.UpdateSilverRecordsAsync(records, cancellationToken);
        }
        
        private async Task ProcessEventsAsync(IEnumerable<SilverRecord> silverRecords,
            CancellationToken cancellationToken)
        {
            var records = silverRecords.ToList();
            logger.LogInformation("Processing {Count} Event records", records.Count);

            // 1. Parse all ProcessedEvent data
            var processedEvents = records
                .Select(r =>
                {
                    try
                    {
                        var evt = System.Text.Json.JsonSerializer.Deserialize<Domain.Modules.DataProcessing.ProcessedEvent>(r.Data);
                        return new { SilverRecord = r, ProcessedEvent = evt };
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to deserialize ProcessedEvent for SilverRecord {RecordId}", r.Id);
                        r.Status = ProcessingStatus.Failed;
                        return null;
                    }
                })
                .Where(x => x?.ProcessedEvent != null &&
                           !string.IsNullOrWhiteSpace(x.ProcessedEvent.Slug) &&
                           !string.IsNullOrWhiteSpace(x.ProcessedEvent.ComedianSlug) &&
                           !string.IsNullOrWhiteSpace(x.ProcessedEvent.VenueSlug))
                .ToList();

            if (processedEvents.Count == 0)
            {
                logger.LogWarning("No valid Event records to process");
                return;
            }

            // 2. Batch query for existing events by slug
            var eventSlugs = processedEvents.Select(x => x.ProcessedEvent!.Slug!).Distinct().ToList();
            var existingEvents = await repository.GetEventsBySlugAsync(eventSlugs, cancellationToken);
            var existingEventsBySlug = existingEvents.ToDictionary(e => e.Slug, e => e);

            // 3. Lookup comedians and venues needed for new events
            var comedianSlugs = processedEvents.Select(x => x.ProcessedEvent!.ComedianSlug!).Distinct().ToList();
            var venueSlugs = processedEvents.Select(x => x.ProcessedEvent!.VenueSlug!).Distinct().ToList();

            var comedians = await repository.GetComediansBySlugAsync(comedianSlugs, cancellationToken);
            var comediansBySlug = comedians.ToDictionary(c => c.Slug, c => c);

            var venues = await repository.GetVenuesBySlugAsync(venueSlugs, cancellationToken);
            var venuesBySlug = venues.ToDictionary(v => v.Slug, v => v);

            // 4. Identify new vs existing events
            var newEvents = new List<Event>();
            var newComedianEvents = new List<ComedianEvent>();
            var updatedEvents = new List<Event>();

            foreach (var item in processedEvents)
            {
                var eventData = item.ProcessedEvent!;

                // Validate comedian and venue exist
                if (!comediansBySlug.TryGetValue(eventData.ComedianSlug!, out var comedian))
                {
                    logger.LogWarning("Comedian with slug {ComedianSlug} not found for event {EventSlug}",
                        eventData.ComedianSlug, eventData.Slug);
                    item.SilverRecord.Status = ProcessingStatus.Failed;
                    continue;
                }

                if (!venuesBySlug.TryGetValue(eventData.VenueSlug!, out var venue))
                {
                    logger.LogWarning("Venue with slug {VenueSlug} not found for event {EventSlug}",
                        eventData.VenueSlug, eventData.Slug);
                    item.SilverRecord.Status = ProcessingStatus.Failed;
                    continue;
                }

                if (existingEventsBySlug.TryGetValue(eventData.Slug!, out var existingEvent))
                {
                    // Update existing event if data changed
                    var hasChanges = false;

                    if (existingEvent.Title != eventData.Title)
                    {
                        existingEvent.Title = eventData.Title!;
                        hasChanges = true;
                    }

                    if (existingEvent.StartDateTime != eventData.StartDateTime)
                    {
                        existingEvent.StartDateTime = eventData.StartDateTime!.Value;
                        hasChanges = true;
                    }

                    if (existingEvent.VenueId != venue.Id)
                    {
                        existingEvent.VenueId = venue.Id;
                        existingEvent.Venue = venue;
                        hasChanges = true;
                    }

                    if (hasChanges)
                    {
                        existingEvent.UpdatedAt = DateTimeOffset.UtcNow;
                        existingEvent.UpdatedBy = "System";
                        updatedEvents.Add(existingEvent);
                    }

                    item.SilverRecord.Status = ProcessingStatus.Completed;
                }
                else if (newEvents.All(ne => ne.Slug != eventData.Slug))
                {
                    // Create new event (check we haven't already added in this batch)
                    var newEvent = new Event
                    {
                        Title = eventData.Title!,
                        Slug = eventData.Slug!,
                        Status = EventStatus.Scheduled,
                        StartDateTime = eventData.StartDateTime!.Value,
                        EndDateTime = eventData.EndDateTime,
                        VenueId = venue.Id,
                        Venue = venue,
                        Source = DataSource.Punchup, // TODO: Get from batch
                        IngestedAt = DateTimeOffset.UtcNow,
                        CreatedAt = DateTimeOffset.UtcNow,
                        CreatedBy = "System",
                        UpdatedAt = DateTimeOffset.UtcNow,
                        UpdatedBy = "System"
                    };

                    newEvents.Add(newEvent);

                    // Create ComedianEvent relationship
                    // Note: We'll need the event ID after insert
                    newComedianEvents.Add(new ComedianEvent
                    {
                        ComedianId = comedian.Id,
                        EventId = newEvent.Id, // This will be set after insert
                        Comedian = comedian,
                        Event = newEvent
                    });

                    item.SilverRecord.Status = ProcessingStatus.Completed;
                }
            }

            // 5. Bulk insert new events
            if (newEvents.Count != 0)
            {
                await repository.AddEventsAsync(newEvents, cancellationToken);
                logger.LogInformation("Created {Count} new events", newEvents.Count);
            }

            // 6. Bulk update existing events
            if (updatedEvents.Count != 0)
            {
                await repository.UpdateEventsAsync(updatedEvents, cancellationToken);
                logger.LogInformation("Updated {Count} existing events", updatedEvents.Count);
            }

            // 7. Create ComedianEvent relationships
            if (newComedianEvents.Count != 0)
            {
                await repository.AddComedianEventsAsync(newComedianEvents, cancellationToken);
                logger.LogInformation("Created {Count} comedian-event relationships", newComedianEvents.Count);
            }

            // 8. Update SilverRecord statuses
            await repository.UpdateSilverRecordsAsync(records, cancellationToken);
        }
        
        private async Task ProcessVenuesAsync(IEnumerable<SilverRecord> silverRecords,
            CancellationToken cancellationToken)
        {
            var records = silverRecords.ToList();
            logger.LogInformation("Processing {Count} Venue records", records.Count);

            // 1. Parse all ProcessedVenue data
            var processedVenues = records
                .Select(r =>
                {
                    try
                    {
                        var venue = System.Text.Json.JsonSerializer.Deserialize<Domain.Modules.DataProcessing.ProcessedVenue>(r.Data);
                        return new { SilverRecord = r, ProcessedVenue = venue };
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to deserialize ProcessedVenue for SilverRecord {RecordId}", r.Id);
                        r.Status = ProcessingStatus.Failed;
                        return null;
                    }
                })
                .Where(x => x?.ProcessedVenue != null && !string.IsNullOrWhiteSpace(x.ProcessedVenue.Slug))
                .ToList();

            if (processedVenues.Count == 0)
            {
                logger.LogWarning("No valid Venue records to process");
                return;
            }

            // 2. Batch query for existing venues by slug
            var slugs = processedVenues.Select(x => x.ProcessedVenue!.Slug!).Distinct().ToList();
            var existingVenues = await repository.GetVenuesBySlugAsync(slugs, cancellationToken);
            var existingVenuesBySlug = existingVenues.ToDictionary(v => v.Slug, v => v);

            // 3. Identify new vs existing
            var newVenues = new List<Venue>();
            var updatedVenues = new List<Venue>();

            foreach (var item in processedVenues)
            {
                var venueData = item.ProcessedVenue!;

                if (existingVenuesBySlug.TryGetValue(venueData.Slug!, out var existingVenue))
                {
                    // Update existing venue if data changed
                    var hasChanges = false;

                    if (existingVenue.Name != venueData.Name)
                    {
                        existingVenue.Name = venueData.Name!;
                        hasChanges = true;
                    }

                    if (hasChanges)
                    {
                        existingVenue.UpdatedAt = DateTimeOffset.UtcNow;
                        existingVenue.UpdatedBy = "System";
                        updatedVenues.Add(existingVenue);
                    }

                    item.SilverRecord.Status = ProcessingStatus.Completed;
                }
                else if (newVenues.All(nv => nv.Slug != venueData.Slug))
                {
                    // Create new venue (check we haven't already added in this batch)
                    var venue = new Venue
                    {
                        Name = venueData.Name!,
                        Slug = venueData.Slug!,
                        Source = DataSource.Punchup, // TODO: Get from batch
                        IngestedAt = DateTimeOffset.UtcNow,
                        CreatedAt = DateTimeOffset.UtcNow,
                        CreatedBy = "System",
                        UpdatedAt = DateTimeOffset.UtcNow,
                        UpdatedBy = "System"
                    };

                    newVenues.Add(venue);
                    item.SilverRecord.Status = ProcessingStatus.Completed;
                }
            }

            // 4. Bulk insert new venues
            if (newVenues.Count != 0)
            {
                await repository.AddVenuesAsync(newVenues, cancellationToken);
                logger.LogInformation("Created {Count} new venues", newVenues.Count);
            }

            // 5. Bulk update existing venues
            if (updatedVenues.Count != 0)
            {
                await repository.UpdateVenuesAsync(updatedVenues, cancellationToken);
                logger.LogInformation("Updated {Count} existing venues", updatedVenues.Count);
            }

            // 6. Update SilverRecord statuses
            await repository.UpdateSilverRecordsAsync(records, cancellationToken);
        }
    }
}