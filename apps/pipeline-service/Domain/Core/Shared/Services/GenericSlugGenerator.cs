namespace ComedyPull.Domain.Core.Shared.Services
{
    public static class GenericSlugGenerator
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
    }
}