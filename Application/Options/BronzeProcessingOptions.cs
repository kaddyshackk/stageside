namespace ComedyPull.Application.Options
{
    /// <summary>
    /// Configuration options for bronze record processing.
    /// </summary>
    public class BronzeProcessingOptions
    {
        /// <summary>
        /// Maximum number of records to process in a single batch.
        /// </summary>
        public int BatchSize { get; set; } = 500;

        /// <summary>
        /// Interval in seconds to flush records even if batch is not full.
        /// </summary>
        public int FlushIntervalSeconds { get; set; } = 10;

        /// <summary>
        /// Timeout in seconds for waiting for new records in the queue.
        /// </summary>
        public int QueueTimeoutSeconds { get; set; } = 5;
    }
}