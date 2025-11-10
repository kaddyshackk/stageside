namespace ComedyPull.Domain.Pipeline
{
    public static class SlugGenerator
    {
        /// <summary>
        /// Generates a URL-friendly slug from a name.
        /// </summary>
        public static string GenerateSlug(string name)
        {
            return name
                .ToLowerInvariant()
                .Replace(" ", "-")
                .Replace("'", "")
                .Replace(".", "")
                .Replace(",", "")
                .Replace("&", "and");
        }
        
        /// <summary>
        /// Generates a unique event slug from comedian, venue, and date.
        /// Format: {comedian-slug}-{venue-slug}-{yyyy-MM-dd}
        /// </summary>
        public static string GenerateEventSlug(string comedianSlug, string venueSlug, DateTimeOffset dateTime)
        {
            var dateString = dateTime.ToString("yyyy-MM-dd");
            return $"{comedianSlug}-{venueSlug}-{dateString}";
        }
    }
}