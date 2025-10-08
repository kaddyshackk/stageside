namespace ComedyPull.Application.Modules.Punchup.Models
{
    /// <summary>
    /// Represents a processed comedian from the data pipeline.
    /// This is an intermediate model used during transformation, separate from the domain Comedian model.
    /// </summary>
    public record ProcessedComedian
    {
        /// <summary>
        /// Gets the comedian name.
        /// </summary>
        public string? Name { get; init; }

        /// <summary>
        /// Gets the slug identifier.
        /// </summary>
        public string? Slug { get; init; }

        /// <summary>
        /// Gets the comedian bio.
        /// </summary>
        public string? Bio { get; init; }

        /// <summary>
        /// Gets the list of events associated with this comedian.
        /// </summary>
        public List<ProcessedEvent>? Events { get; init; }

        /// <summary>
        /// Gets the source-specific comedian ID from Punchup.
        /// </summary>
        public string? SourceId { get; init; }

        /// <summary>
        /// Gets the timestamp when this record was processed.
        /// </summary>
        public DateTimeOffset? ProcessedAt { get; init; }
    }
}
