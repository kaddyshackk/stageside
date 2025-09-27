namespace ComedyPull.Application.Modules.DataSync.Configuration
{
    /// <summary>
    /// Defines the options for the DataSync module.
    /// </summary>
    public class DataSyncOptions
    {
        /// <summary>
        /// Gets the options for source record ingestion.
        /// </summary>
        public SourceRecordIngestionOptions SourceRecordIngestion { get; init; } = new();

        /// <summary>
        /// Gets the options for punchup collection.
        /// </summary>
        public PunchupCollectionOptions PunchupCollection { get; init; } = new();
    }
}