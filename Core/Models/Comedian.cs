namespace ComedyPull.Domain.Models
{
    /// <summary>
    /// Represents a standup comedian.
    /// </summary>
    public record Comedian
    {
        /// <summary>
        /// Gets and sets the identifier.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Gets and sets the slug identifier.
        /// </summary>
        public required string Slug { get; set; }

        /// <summary>
        /// Gets and sets the name.
        /// </summary>
        public required string Name { get; set; }
        
        /// <summary>
        /// Gets and sets the comedian bio.
        /// </summary>
        public required string Bio { get; set; }
    }
}
