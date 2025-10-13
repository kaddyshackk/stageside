namespace ComedyPull.Application.Modules.DataSync.Options
{
    /// <summary>
    /// Defines the options for punchup collection.
    /// </summary>
    public class PunchupCollectionOptions
    {
        /// <summary>
        /// Gets the concurrency for punchup collection.
        /// </summary>
        public int Concurrency { get; init; } = 5;
    }
}