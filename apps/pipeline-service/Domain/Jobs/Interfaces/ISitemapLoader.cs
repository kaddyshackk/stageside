namespace ComedyPull.Domain.Jobs.Interfaces
{
    public interface ISitemapLoader
    {
        /// <summary>
        /// Loads a sitemap from the given URL and returns a list of URLs found in it.
        /// </summary>
        /// <param name="sitemapUrl">Url of sitemap to load</param>
        /// <returns>Returns a list of URLs parsed from the sitemap.</returns>
        public Task<List<string>> LoadSitemapAsync(string sitemapUrl);
    }
}