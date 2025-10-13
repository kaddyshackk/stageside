using System.Xml.Linq;
using ComedyPull.Application.Modules.DataSync.Interfaces;

namespace ComedyPull.Application.Modules.DataSync
{
    public class SitemapLoader(IHttpClientFactory httpClientFactory) : ISitemapLoader
    {
        private const string NamespaceUrl = "http://www.sitemaps.org/schemas/sitemap/0.9";

        /// <summary>
        /// Loads a sitemap from the given URL and returns a list of URLs found in it.
        /// </summary>
        /// <param name="sitemapUrl">Url of sitemap to load</param>
        /// <returns>Returns a list of URLs parsed from the sitemap.</returns>
        public async Task<List<string>> LoadSitemapAsync(string sitemapUrl)
        {
            using var client = httpClientFactory.CreateClient();
            var response = await client.GetStringAsync(sitemapUrl);
            var sitemap = XDocument.Parse(response);
            var ns = XNamespace.Get(NamespaceUrl);
            return sitemap.Descendants(ns + "url")
                .Select(url => url.Element(ns + "loc")?.Value)
                .OfType<string>()
                .ToList();
        }
    }
}