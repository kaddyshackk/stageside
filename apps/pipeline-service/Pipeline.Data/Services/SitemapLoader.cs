using System.Text.RegularExpressions;
using System.Xml.Linq;
using StageSide.Pipeline.Domain.Scheduling.Interfaces;
using StageSide.Pipeline.Domain.Scheduling.Models;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace StageSide.Pipeline.Data.Services
{
    public class SitemapLoader(IHttpClientFactory httpClientFactory, ILogger<SitemapLoader> logger) : ISitemapLoader
    {
        private const string NamespaceUrl = "http://www.sitemaps.org/schemas/sitemap/0.9";

        /// inheritdoc
        public async Task<ICollection<string>> LoadSitemapAsync(Sitemap sitemap)
        {
            using (LogContext.PushProperty("SitemapId", sitemap.Id))
            using (LogContext.PushProperty("SitemapUrl", sitemap.Url))
            using (LogContext.PushProperty("JobId", sitemap.JobId))
            {
                try
                {
                    using var client = httpClientFactory.CreateClient();
                    var response = await client.GetStringAsync(sitemap.Url);
                
                    logger.LogInformation("Loaded sitemap.");
                
                    var urls = ParseSitemap(response);
                    var filtered = FilterUrlsByRegex(urls, sitemap.RegexFilter);

                    return filtered;
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

        /// inheritdoc
        public async Task<ICollection<string>> LoadManySitemapsAsync(IEnumerable<Sitemap> sitemaps)
        {
            var results = await Task.WhenAll(sitemaps.Select(LoadSitemapAsync));
            return results.SelectMany(g => g).ToList();
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

        private static List<string> FilterUrlsByRegex(List<string> urls, string? filter)
        {
            if (string.IsNullOrEmpty(filter)) return urls;
            var regex = new Regex(filter, RegexOptions.Compiled);
            return urls.Where(url => regex.IsMatch(url)).ToList();
        }
    }
}