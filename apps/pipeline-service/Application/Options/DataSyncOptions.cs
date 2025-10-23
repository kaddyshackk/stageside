namespace ComedyPull.Application.Options
{
    /// <summary>
    /// Defines the options for the DataSync module.
    /// </summary>
    public class DataSyncOptions
    {
        /// <summary>
        /// Gets the options for source record ingestion.
        /// </summary>
        public IngestionOptions Ingestion { get; init; } = new();
    }
}