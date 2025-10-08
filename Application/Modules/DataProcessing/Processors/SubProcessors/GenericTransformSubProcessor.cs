using ComedyPull.Application.Modules.DataProcessing.Processors.Interfaces;
using ComedyPull.Domain.Enums;
using ComedyPull.Domain.Models.Processing;
using Microsoft.Extensions.Logging;

namespace ComedyPull.Application.Modules.DataProcessing.Processors.SubProcessors
{
    public class GenericTransformSubProcessor(ILogger<GenericTransformSubProcessor> logger)
        : ISubProcessor<DataSource>
    {
        public DataSource? Key => null; // null = generic/fallback
        public ProcessingState FromState => ProcessingState.Ingested;
        public ProcessingState ToState => ProcessingState.Transformed;

        public Task ProcessAsync(IEnumerable<SourceRecord> records, CancellationToken cancellationToken)
        {
            logger.LogInformation("Processing {Count} records with generic transform logic", records.Count());

            // Generic transformation logic
            foreach (var record in records)
            {
                logger.LogDebug("Transforming record {RecordId}", record.Id);
                // TODO: Implement generic transformation
            }

            return Task.CompletedTask;
        }
    }
}