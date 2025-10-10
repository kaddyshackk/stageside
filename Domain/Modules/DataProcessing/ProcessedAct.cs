namespace ComedyPull.Domain.Modules.DataProcessing
{
    /// <summary>
    /// Represents a processed act from the data pipeline.
    /// This is an intermediate model used during transformation, separate from the domain Comedian model.
    /// </summary>
    public record ProcessedAct
    {
        /// <summary>
        /// Gets the act name.
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
        /// Gets the source-specific act ID from Punchup.
        /// </summary>
        public string? SourceId { get; init; }

        /// <summary>
        /// Gets the timestamp when this record was processed.
        /// </summary>
        public DateTimeOffset? ProcessedAt { get; init; }
    }
}
