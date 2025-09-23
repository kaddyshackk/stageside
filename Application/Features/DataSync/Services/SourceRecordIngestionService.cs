using ComedyPull.Application.Features.DataSync.Interfaces;
using ComedyPull.Application.Interfaces;
using ComedyPull.Application.Options;
using ComedyPull.Domain.Models.Processing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;

namespace ComedyPull.Application.Features.DataSync.Services
{
    /// <summary>
    /// Background service that processes bronze records from Redis queue in batches.
    /// </summary>
    public class SourceRecordIngestionService(
        IQueue<SourceRecord> queue,
        ISourceRecordWriteRepository writeRepository,
        ILogger<SourceRecordIngestionService> logger,
        IOptions<BronzeProcessingOptions> options
    ) : BackgroundService
    {
        private readonly BronzeProcessingOptions _options = options.Value;
        private readonly List<SourceRecord> _currentBatch = [];
        private DateTime _lastFlushTime = DateTime.UtcNow;

        /// <summary>
        /// Main execution loop for processing bronze records.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Bronze Record Processing Service started");

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessBatchAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error in bronze record processing loop");
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                }
            }

            await FlushCurrentBatchAsync(cancellationToken);

            logger.LogInformation("Bronze Record Processing Service stopped");
        }

        /// <summary>
        /// Processes a batch of records from the queue.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        private async Task ProcessBatchAsync(CancellationToken cancellationToken)
        {
            var timeout = TimeSpan.FromSeconds(_options.QueueTimeoutSeconds);
            var availableSpace = _options.BatchSize - _currentBatch.Count;

            // Dequeue records up to available space in current batch
            var records = await queue.DequeueAsync(availableSpace, timeout, cancellationToken);

            if (records.Count != 0)
            {
                _currentBatch.AddRange(records);
                logger.LogDebug("Added {Count} records to batch. Current batch size: {BatchSize}",
                    records.Count, _currentBatch.Count);
            }

            // Check if we should flush the batch
            var shouldFlushBySize = _currentBatch.Count >= _options.BatchSize;
            var shouldFlushByTime =
                DateTime.UtcNow - _lastFlushTime >= TimeSpan.FromSeconds(_options.FlushIntervalSeconds);

            if (_currentBatch.Count != 0 && (shouldFlushBySize || shouldFlushByTime))
            {
                await FlushCurrentBatchAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Flushes the current batch to the database.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        private async Task FlushCurrentBatchAsync(CancellationToken cancellationToken)
        {
            if (_currentBatch.Count == 0)
                return;
            try
            {
                logger.LogInformation("Flushing batch of {Count} bronze records to database", _currentBatch.Count);

                var batchSize = _currentBatch.Count;
                await writeRepository.BatchInsertAsync(_currentBatch, cancellationToken);

                logger.LogInformation("Successfully processed {Count} bronze records", batchSize);

                // Clear the batch and update flush time
                _currentBatch.Clear();
                _lastFlushTime = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to flush bronze records batch of size {Count}", _currentBatch.Count);

                // TODO: Replace with better error handling
                _currentBatch.Clear();
                _lastFlushTime = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Gets the current status of the processing service.
        /// </summary>
        public async Task<ProcessingStatus> GetStatusAsync()
        {
            var queueLength = await queue.GetQueueLengthAsync();

            return new ProcessingStatus
            {
                QueueLength = queueLength,
                CurrentBatchSize = _currentBatch.Count,
                LastFlushTime = _lastFlushTime,
                NextFlushTime = _lastFlushTime.AddSeconds(_options.FlushIntervalSeconds)
            };
        }
    }

    /// <summary>
    /// Status information for the bronze record processing service.
    /// </summary>
    public record ProcessingStatus
    {
        public long QueueLength { get; init; }
        public int CurrentBatchSize { get; init; }
        public DateTime LastFlushTime { get; init; }
        public DateTime NextFlushTime { get; init; }
    }
}