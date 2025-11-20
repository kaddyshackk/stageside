using StageSide.SpaCollector.Domain.Collection.Models;

namespace StageSide.SpaCollector.Domain.Collection.Interfaces
{
    public interface ISitemapLoader
    {
        /// <summary>
        /// Loads a sitemap from its URL.
        /// </summary>
        /// <param name="sitemap">Sitemap to load.</param>
        /// <returns>A collection of loaded URLs.</returns>
        public Task<ICollection<string>> LoadSitemapAsync(Sitemap sitemap);
        
        /// <summary>
        /// Loads multiple sitemaps from their url and flattens the results.
        /// </summary>
        /// <param name="sitemaps">Sitemaps to load.</param>
        /// <returns>A collection of loaded URLs.</returns>
        public Task<ICollection<string>> LoadManySitemapsAsync(IEnumerable<Sitemap> sitemaps);
    }
}