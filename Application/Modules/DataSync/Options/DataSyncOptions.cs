namespace ComedyPull.Application.Modules.DataSync.Options
{
    /// <summary>
    /// Defines the options for the DataSync module.
    /// </summary>
    public class DataSyncOptions
    {
        /// <summary>
        /// Gets the options for source record ingestion.
        /// </summary>
        public BronzeRecordIngestionOptions BronzeRecordIngestion { get; init; } = new();

        /// <summary>
        /// Gets the options for punchup collection.
        /// </summary>
        public PunchupCollectionOptions PunchupCollection { get; init; } = new();
    }
}