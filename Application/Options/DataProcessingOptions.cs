namespace ComedyPull.Application.Options
{
    /// <summary>
    /// Defines the data processing options
    /// </summary>
    public class DataProcessingOptions
    {
        /// <summary>
        /// Gets the processing options for Bronze Records.
        /// </summary>
        public BronzeProcessingOptions Bronze { get; init; } = new();
    }
}