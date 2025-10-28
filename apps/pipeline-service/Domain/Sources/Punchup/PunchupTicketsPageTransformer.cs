using System.Text.Json;
using ComedyPull.Domain.Interfaces;
using ComedyPull.Domain.Models;
using ComedyPull.Domain.Models.Pipeline;
using ComedyPull.Domain.Services;
using ComedyPull.Domain.Sources.Punchup.Models;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace ComedyPull.Domain.Sources.Punchup
{
    public class PunchupTicketsPageTransformer(ILogger<PunchupTicketsPageTransformer> logger) : ITransformer
    {
        public IEnumerable<SilverRecord> Transform(BronzeRecord record)
        {
            var silverRecords = new List<SilverRecord>();

            using (LogContext.PushProperty("RecordId", record.Id))
            {
                PunchupRecord? punchupRecord;
                try
                {
                    punchupRecord = JsonSerializer.Deserialize<PunchupRecord>(record.Data);
                }
                catch (JsonException ex)
                {
                    logger.LogError(ex, "Failed to deserialize PunchupRecord from BronzeRecord {RecordId}", record.Id);
                    record.State = ProcessingState.Failed;
                    return [];
                }

                if (punchupRecord == null)
                {
                    logger.LogWarning("Failed to deserialize PunchupRecord from BronzeRecord {RecordId}", record.Id);
                    record.State = ProcessingState.Failed;
                    return [];
                }

                // Generate comedian slug
                var comedianSlug = SlugGenerator.GenerateSlug(punchupRecord.Name);

                var now = DateTimeOffset.UtcNow;
                
                // 1. Create SilverRecord for Act
                var processedAct = new ProcessedAct
                {
                    Name = punchupRecord.Name,
                    Slug = comedianSlug,
                    Bio = punchupRecord.Bio,
                    ProcessedAt = now
                };

                using (LogContext.PushProperty("@ProcessedAct", processedAct))
                {
                    silverRecords.Add(new SilverRecord
                    {
                        BatchId = record.BatchId,
                        BronzeRecordId = record.Id,
                        EntityType = EntityType.Act,
                        State = ProcessingState.Pending,
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
                        .ToList();

                    silverRecords.AddRange(from venueName in uniqueVenues
                        let venueSlug = SlugGenerator.GenerateSlug(venueName)
                        select new ProcessedVenue { Name = venueName, Slug = venueSlug, Location = punchupRecord.Events?.FirstOrDefault(e => e.Venue == venueName)?.Location, ProcessedAt = DateTimeOffset.UtcNow }
                        into processedVenue
                        select new SilverRecord
                        {
                            BatchId = record.BatchId,
                            BronzeRecordId = record.Id,
                            EntityType = EntityType.Venue,
                            State = ProcessingState.Pending,
                            Data = JsonSerializer.Serialize(processedVenue),
                            CreatedAt = now,
                            CreatedBy = "System",
                            UpdatedAt = now,
                            UpdatedBy = "System",
                        });

                    // 3. Create SilverRecords for each Event
                    silverRecords.AddRange(from punchupEvent in punchupRecord.Events
                        let venueSlug = SlugGenerator.GenerateSlug(punchupEvent.Venue)
                        let eventSlug = SlugGenerator.GenerateEventSlug(comedianSlug, venueSlug, punchupEvent.StartDateTime)
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
                            BatchId = record.BatchId,
                            BronzeRecordId = record.Id,
                            EntityType = EntityType.Event,
                            State = ProcessingState.Pending,
                            Data = JsonSerializer.Serialize(processedEvent),
                            CreatedAt = now,
                            CreatedBy = "System",
                            UpdatedAt = now,
                            UpdatedBy = "System",
                        });

                        // Mark bronze record as transformed
                        record.State = ProcessingState.Pending;

                        var createdSilverRecordCount = 1 + uniqueVenues.Count + punchupRecord.Events.Count;
                        using (LogContext.PushProperty("VenueCount", uniqueVenues.Count))
                        using (LogContext.PushProperty("EventCount", punchupRecord.Events.Count))
                        using (LogContext.PushProperty("SilverRecordCount", createdSilverRecordCount))
                        {
                            logger.LogDebug("Flattened PunchupRecord for comedian {Name} into {Count} SilverRecords",
                                punchupRecord.Name, createdSilverRecordCount);
                        }
                }
            }
            
            return silverRecords;
        }
    }
}