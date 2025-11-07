namespace ComedyPull.Domain.Core.Events.Services
{
    public static class EventSlugGenerator
    {
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