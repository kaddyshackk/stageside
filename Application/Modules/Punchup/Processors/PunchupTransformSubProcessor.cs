using ComedyPull.Application.Modules.DataProcessing.Steps.Interfaces;
using ComedyPull.Application.Modules.DataProcessing.Steps.Transform.Interfaces;
using ComedyPull.Application.Modules.Punchup.Models;
using ComedyPull.Domain.Enums;
using ComedyPull.Domain.Modules.DataProcessing;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ComedyPull.Application.Modules.Punchup.Processors
{
    public class PunchupTransformSubProcessor(
        ITransformStateRepository repository,
        ILogger<PunchupTransformSubProcessor> logger)
        : ISubProcessor<DataSourceType>
    {
        public DataSourceType? Key => DataSourceType.PunchupTicketsPage;
        public ProcessingState FromState => ProcessingState.Ingested;
        public ProcessingState ToState => ProcessingState.Transformed;

        public async Task ProcessAsync(IEnumerable<BronzeRecord> records, CancellationToken cancellationToken)
        {
            var bronzeRecords = records as BronzeRecord[] ?? records.ToArray();
            logger.LogInformation("Processing {Count} Punchup tickets page records - flattening into SilverRecords", bronzeRecords.Count());

            var silverRecordsToCreate = new List<SilverRecord>();

            foreach (var bronzeRecord in bronzeRecords)
            {
                try
                {
                    var punchupRecord = JsonSerializer.Deserialize<PunchupRecord>(bronzeRecord.Data);

                    if (punchupRecord == null)
                    {
                        logger.LogWarning("Failed to deserialize PunchupRecord for BronzeRecord {RecordId}", bronzeRecord.Id);
                        bronzeRecord.Status = ProcessingStatus.Failed;
                        continue;
                    }

                    // Generate comedian slug
                    var comedianSlug = GenerateSlug(punchupRecord.Name);

                    var now = DateTimeOffset.UtcNow;
                    
                    // 1. Create SilverRecord for Comedian (Act)
                    var processedAct = new ProcessedAct
                    {
                        Name = punchupRecord.Name,
                        Slug = comedianSlug,
                        Bio = punchupRecord.Bio,
                        ProcessedAt = now
                    };

                    silverRecordsToCreate.Add(new SilverRecord
                    {
                        BatchId = bronzeRecord.BatchId,
                        BronzeRecordId = bronzeRecord.Id,
                        EntityType = EntityType.Act,
                        Status = ProcessingStatus.Processing,
                        Data = JsonSerializer.Serialize(processedAct),
                        CreatedAt = now,
                        CreatedBy = "System",
                        UpdatedAt = now,
                        UpdatedBy = "System",
                    });

                    // 2. Create SilverRecords for each unique Venue
                    var uniqueVenues = punchupRecord.Events
                        .Select(e => e.Venue)
                        .Distinct()
                        .ToList() ?? [];

                    silverRecordsToCreate.AddRange(from venueName in uniqueVenues
                    let venueSlug = GenerateSlug(venueName)
                    select new ProcessedVenue { Name = venueName, Slug = venueSlug, Location = punchupRecord.Events?.FirstOrDefault(e => e.Venue == venueName)?.Location, ProcessedAt = DateTimeOffset.UtcNow }
                    into processedVenue
                    select new SilverRecord
                    {
                        BatchId = bronzeRecord.BatchId,
                        BronzeRecordId = bronzeRecord.Id,
                        EntityType = EntityType.Venue,
                        Status = ProcessingStatus.Processing,
                        Data = JsonSerializer.Serialize(processedVenue),
                        CreatedAt = now,
                        CreatedBy = "System",
                        UpdatedAt = now,
                        UpdatedBy = "System",
                    });

                    // 3. Create SilverRecords for each Event
                    silverRecordsToCreate.AddRange(from punchupEvent in punchupRecord.Events ?? Enumerable.Empty<PunchupEvent>()
                        let venueSlug = GenerateSlug(punchupEvent.Venue)
                        let eventSlug = GenerateEventSlug(comedianSlug, venueSlug, punchupEvent.StartDateTime)
                        select new ProcessedEvent
                        {
                            Title = $"{punchupRecord.Name} at {punchupEvent.Venue}",
                            Slug = eventSlug,
                            ComedianSlug = comedianSlug,
                            VenueSlug = venueSlug,
                            StartDateTime = punchupEvent.StartDateTime,
                            EndDateTime = null,
                            TicketLink = punchupEvent.TicketLink
                        }
                        into processedEvent
                        select new SilverRecord
                        {
                            BatchId = bronzeRecord.BatchId,
                            BronzeRecordId = bronzeRecord.Id,
                            EntityType = EntityType.Event,
                            Status = ProcessingStatus.Processing,
                            Data = JsonSerializer.Serialize(processedEvent),
                            CreatedAt = now,
                            CreatedBy = "System",
                            UpdatedAt = now,
                            UpdatedBy = "System",
                        });

                    // Mark bronze record as transformed
                    bronzeRecord.Status = ProcessingStatus.Completed;

                    logger.LogDebug("Flattened PunchupRecord for comedian {Name} into {Count} SilverRecords",
                        punchupRecord.Name, 1 + uniqueVenues.Count + (punchupRecord.Events?.Count ?? 0));
                }
                catch (JsonException ex)
                {
                    logger.LogError(ex, "JSON deserialization failed for BronzeRecord {RecordId}", bronzeRecord.Id);
                    bronzeRecord.Status = ProcessingStatus.Failed;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unexpected error transforming BronzeRecord {RecordId}", bronzeRecord.Id);
                    bronzeRecord.Status = ProcessingStatus.Failed;
                }
            }

            // Bulk create all SilverRecords
            if (silverRecordsToCreate.Count != 0)
            {
                await repository.CreateSilverRecordsAsync(silverRecordsToCreate, cancellationToken);
                logger.LogInformation("Created {Count} SilverRecords from {BronzeCount} BronzeRecords",
                    silverRecordsToCreate.Count, bronzeRecords.Length);
            }

            // Update BronzeRecords statuses
            await repository.UpdateBronzeRecordsAsync(bronzeRecords, cancellationToken);
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
                .Replace(".", "")
                .Replace(",", "")
                .Replace("&", "and");
        }

        /// <summary>
        /// Generates a unique event slug from comedian, venue, and date.
        /// Format: {comedian-slug}-{venue-slug}-{yyyy-MM-dd}
        /// </summary>
        private static string GenerateEventSlug(string comedianSlug, string venueSlug, DateTimeOffset dateTime)
        {
            var dateString = dateTime.ToString("yyyy-MM-dd");
            return $"{comedianSlug}-{venueSlug}-{dateString}";
        }
    }
}