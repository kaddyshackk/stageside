using System.Xml.Linq;

namespace ComedyPull.Application.DataSync.Services
{
    public class SitemapLoader(HttpClient httpClient)
    {
        private static readonly string NamespaceUrl = "http://www.sitemaps.org/schemas/sitemap/0.9";

        /// <summary>
        /// Loads a sitemap from the given URL and returns a list of URLs found in it.
        /// </summary>
        /// <param name="sitemapUrl">Url of sitemap to load</param>
        /// <returns>Returns a list of URLs parsed from the sitemap.</returns>
        public async Task<List<string>> LoadSitemapAsync(string sitemapUrl)
        {
            var response = await httpClient.GetStringAsync(sitemapUrl);
            if (response is null) return [];
            var sitemap = XDocument.Parse(response);
            var ns = XNamespace.Get(NamespaceUrl);
            return sitemap.Descendants(ns + "url")
                .Select(url => url.Element(ns + "loc")?.Value)
                .OfType<string>()
                .ToList();
        }
    }
}
