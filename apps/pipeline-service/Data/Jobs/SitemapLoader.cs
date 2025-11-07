using System.Xml.Linq;
using ComedyPull.Domain.Jobs.Interfaces;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace ComedyPull.Data.Jobs
{
    public class SitemapLoader(IHttpClientFactory httpClientFactory, ILogger<SitemapLoader> logger) : ISitemapLoader
    {
        private const string NamespaceUrl = "http://www.sitemaps.org/schemas/sitemap/0.9";

        /// <summary>
        /// Loads a sitemap from the given URL and returns a list of URLs found in it.
        /// </summary>
        /// <param name="sitemapUrl">Url of sitemap to load</param>
        /// <returns>Returns a list of URLs parsed from the sitemap.</returns>
        public async Task<List<string>> LoadSitemapAsync(string sitemapUrl)
        {
            using (LogContext.PushProperty("SitemapUrl", sitemapUrl))
            {
                logger.LogDebug("Loading sitemap.");

                try
                {
                    using var client = httpClientFactory.CreateClient();
                    var response = await client.GetStringAsync(sitemapUrl);
                
                    logger.LogInformation("Loaded sitemap.");
                
                    return ParseSitemap(response);
                }
                catch (HttpRequestException ex)
                {
                    logger.LogError(
                        ex,
                        "HTTP error loading sitemap"
                    );
                    throw;
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "Unexpected error loading sitemap"
                    );
                    throw;
                }
            }
        }

        private static List<string> ParseSitemap(string sitemap)
        {
            var s = XDocument.Parse(sitemap);
            var ns = XNamespace.Get(NamespaceUrl);
            return s.Descendants(ns + "url")
                .Select(url => url.Element(ns + "loc")?.Value)
                .OfType<string>()
                .ToList();
        }
    }
}