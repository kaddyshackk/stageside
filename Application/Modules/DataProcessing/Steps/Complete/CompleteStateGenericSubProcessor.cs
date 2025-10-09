using ComedyPull.Application.Modules.Punchup.Models;
using ComedyPull.Domain.Enums;
using ComedyPull.Domain.Models;
using ComedyPull.Domain.Models.Processing;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using ComedyPull.Application.Modules.DataProcessing.Steps.Complete.Interfaces;
using ComedyPull.Application.Modules.DataProcessing.Steps.Interfaces;

namespace ComedyPull.Application.Modules.DataProcessing.Steps.Complete
{
    public class CompleteStateGenericSubProcessor(
        ICompleteStateRepository repository,
        ILogger<CompleteStateGenericSubProcessor> logger) : ISubProcessor<DataSource>
    {
        public DataSource? Key => null;
        public ProcessingState FromState => ProcessingState.Transformed;
        public ProcessingState ToState => ProcessingState.Completed;

        public async Task ProcessAsync(IEnumerable<SourceRecord> records, CancellationToken cancellationToken)
        {
            logger.LogInformation("Processing {Count} records for entity persistence", records.Count());

            foreach (var record in records)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(record.ProcessedData))
                    {
                        logger.LogWarning("ProcessedData is empty for SourceRecord {RecordId}", record.Id);
                        record.Status = ProcessingStatus.Failed;
                        continue;
                    }

                    // Process based on entity type
                    switch (record.EntityType)
                    {
                        case EntityType.Act:
                            await ProcessComedianAsync(record, cancellationToken);
                            break;
                        case EntityType.Event:
                            logger.LogWarning("Standalone Event processing not yet implemented for SourceRecord {RecordId}", record.Id);
                            record.Status = ProcessingStatus.Failed;
                            break;
                        case EntityType.Venue:
                            logger.LogWarning("Venue processing not yet implemented for SourceRecord {RecordId}", record.Id);
                            record.Status = ProcessingStatus.Failed;
                            break;
                        default:
                            logger.LogWarning("Unknown EntityType {EntityType} for SourceRecord {RecordId}", record.EntityType, record.Id);
                            record.Status = ProcessingStatus.Failed;
                            break;
                    }
                }
                catch (JsonException ex)
                {
                    logger.LogError(ex, "JSON deserialization failed for SourceRecord {RecordId}", record.Id);
                    record.Status = ProcessingStatus.Failed;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unexpected error persisting SourceRecord {RecordId}", record.Id);
                    record.Status = ProcessingStatus.Failed;
                }
            }
        }

        private async Task ProcessComedianAsync(SourceRecord record, CancellationToken cancellationToken)
        {
            var processedComedian = JsonSerializer.Deserialize<ProcessedComedian>(record.ProcessedData!);

            if (processedComedian == null || string.IsNullOrWhiteSpace(processedComedian.Name))
            {
                logger.LogWarning("Invalid ProcessedComedian data for SourceRecord {RecordId}", record.Id);
                record.Status = ProcessingStatus.Failed;
                return;
            }

            // Check if comedian already exists by slug
            var existingComedian = await repository.GetComedianBySlugAsync(processedComedian.Slug!, cancellationToken);

            Comedian comedian;
            if (existingComedian != null)
            {
                logger.LogDebug("Comedian {Slug} already exists, using existing record", processedComedian.Slug);
                comedian = existingComedian;
            }
            else
            {
                // Create new comedian
                comedian = new Comedian
                {
                    Name = processedComedian.Name,
                    Slug = processedComedian.Slug!,
                    Bio = processedComedian.Bio ?? string.Empty,
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = "System",
                    UpdatedAt = DateTimeOffset.UtcNow,
                    UpdatedBy = "System"
                };

                await repository.AddComedian(comedian);
                logger.LogDebug("Created new Comedian {Name} with slug {Slug}", comedian.Name, comedian.Slug);
            }

            // Process events associated with the comedian
            if (processedComedian.Events != null && processedComedian.Events.Any())
            {
                foreach (var processedEvent in processedComedian.Events)
                {
                    await ProcessEventForComedianAsync(comedian, processedEvent, cancellationToken);
                }
            }

            // Mark record as completed
            record.State = ProcessingState.Completed;
            record.Status = ProcessingStatus.Completed;

            logger.LogDebug("Successfully persisted comedian {Name} with {EventCount} events",
                processedComedian.Name, processedComedian.Events?.Count ?? 0);
        }

        private async Task ProcessEventForComedianAsync(
            Comedian comedian,
            ProcessedEvent processedEvent,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(processedEvent.Title) ||
                processedEvent.StartDateTime == null ||
                string.IsNullOrWhiteSpace(processedEvent.Venue))
            {
                logger.LogWarning("Invalid event data for comedian {ComedianId}, skipping event", comedian.Id);
                return;
            }

            // Find or create venue
            var venueSlug = GenerateSlug(processedEvent.Venue);
            var venue = await repository.GetVenueBySlugAsync(venueSlug, cancellationToken);

            if (venue == null)
            {
                venue = new Venue
                {
                    Name = processedEvent.Venue,
                    Slug = venueSlug,
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = "System",
                    UpdatedAt = DateTimeOffset.UtcNow,
                    UpdatedBy = "System"
                };

                await repository.AddVenue(venue);
                logger.LogDebug("Created new Venue {Name} with slug {Slug}", venue.Name, venue.Slug);
            }

            // Create event
            var eventEntity = new Event
            {
                Title = processedEvent.Title,
                Status = EventStatus.Scheduled,
                StartDateTime = processedEvent.StartDateTime.Value,
                EndDateTime = processedEvent.EndDateTime,
                VenueId = venue.Id,
                Venue = venue,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "System",
                UpdatedAt = DateTimeOffset.UtcNow,
                UpdatedBy = "System"
            };

            await repository.AddEvent(eventEntity);
            logger.LogDebug("Created new Event {Title} at {Venue}", eventEntity.Title, venue.Name);

            var comedianEvent = new ComedianEvent
            {
                ComedianId = comedian.Id,
                EventId = eventEntity.Id,
                Comedian = comedian,
                Event = eventEntity
            };

            await repository.AddComedianEvent(comedianEvent);
        }

        /// <summary>
        /// Generates a URL-friendly slug from a name.
        /// </summary>
        private static string GenerateSlug(string name)
        {
            return name
                .ToLowerInvariant()
                .Replace(" ", "-")
                .Replace("'", "")
                .Replace(".", "");
        }
    }
}
