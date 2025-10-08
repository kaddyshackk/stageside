using ComedyPull.Application.Modules.DataProcessing.Processors.Interfaces;
using ComedyPull.Application.Modules.Punchup.Models;
using ComedyPull.Domain.Enums;
using ComedyPull.Domain.Models.Processing;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ComedyPull.Application.Modules.Punchup.Processors.SubProcessors
{
    public class PunchupTransformSubProcessor(ILogger<PunchupTransformSubProcessor> logger)
        : ISubProcessor<DataSource>
    {
        public DataSource? Key => DataSource.Punchup;
        public ProcessingState FromState => ProcessingState.Ingested;
        public ProcessingState ToState => ProcessingState.Transformed;

        public Task ProcessAsync(IEnumerable<SourceRecord> records, CancellationToken cancellationToken)
        {
            logger.LogInformation("Processing {Count} Punchup records with source-specific transform logic", records.Count());

            foreach (var record in records)
            {
                try
                {
                    var punchupRecord = JsonSerializer.Deserialize<PunchupRecord>(record.RawData);

                    if (punchupRecord == null)
                    {
                        logger.LogWarning("Failed to deserialize PunchupRecord for SourceRecord {RecordId}", record.Id);
                        record.Status = ProcessingStatus.Failed;
                        continue;
                    }

                    var processedComedian = new ProcessedComedian
                    {
                        Name = punchupRecord.Name,
                        Slug = GenerateSlug(punchupRecord.Name),
                        Bio = punchupRecord.Bio,
                        Events = punchupRecord.Events?.Select(e => new ProcessedEvent
                        {
                            Title = $"{punchupRecord.Name} at {e.Venue}",
                            StartDateTime = e.StartDateTime,
                            EndDateTime = null,
                            Location = e.Location,
                            Venue = e.Venue,
                            TicketLink = e.TicketLink,
                            SourceId = null
                        }).ToList(),
                        SourceId = null,
                        ProcessedAt = DateTimeOffset.UtcNow
                    };

                    // Serialize to ProcessedData
                    record.ProcessedData = JsonSerializer.Serialize(processedComedian);
                    record.State = ProcessingState.Transformed;
                    record.Status = ProcessingStatus.Processing;

                    logger.LogDebug("Successfully transformed PunchupRecord for comedian {Name}", punchupRecord.Name);
                }
                catch (JsonException ex)
                {
                    logger.LogError(ex, "JSON deserialization failed for SourceRecord {RecordId}", record.Id);
                    record.Status = ProcessingStatus.Failed;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unexpected error transforming SourceRecord {RecordId}", record.Id);
                    record.Status = ProcessingStatus.Failed;
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Generates a URL-friendly slug from a comedian name.
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