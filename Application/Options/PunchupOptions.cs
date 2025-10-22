namespace ComedyPull.Application.Options
{
    /// <summary>
    /// Defines the options for punchup collection.
    /// </summary>
    public class PunchupOptions
    {
        /// <summary>
        /// Gets the concurrency for punchup collection.
        /// </summary>
        public int Concurrency { get; init; } = 5;
    }
}