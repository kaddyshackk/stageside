namespace ComedyPull.Application.Options
{
    /// <summary>
    /// Configuration options for source record ingestion.
    /// </summary>
    public class IngestionOptions
    {
        /// <summary>
        /// Maximum number of records to process in a single batch.
        /// </summary>
        public int BatchSize { get; init; } = 500;

        /// <summary>
        /// Interval in seconds to flush records even if batch is not full.
        /// </summary>
        public int FlushIntervalSeconds { get; init; } = 10;

        /// <summary>
        /// Timeout in seconds for waiting for new records in the queue.
        /// </summary>
        public int QueueTimeoutSeconds { get; init; } = 5;
    }
}