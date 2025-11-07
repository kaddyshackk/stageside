using System.Text.RegularExpressions;
using ComedyPull.Domain.Jobs.Interfaces;

namespace ComedyPull.Domain.Jobs.Services
{
    public class SitemapService(ISitemapLoader sitemapLoader, IJobSitemapRepository sitemapRepository)
    {
        public async Task<List<string>> GetSitemapUrlsForJobAsync(Guid jobId, CancellationToken stoppingToken)
        {
            var sitemaps = await sitemapRepository.ReadJobSitemapsForJobAsync(jobId, stoppingToken);
            return await GetSitemapUrlsAsync(sitemaps);
        }
        
        private async Task<List<string>> GetSitemapUrlsAsync(ICollection<Sitemap> sitemaps)
        {
            var allUrls = new List<string>();
            if (sitemaps.Count != 0)
            {
                foreach (var sitemap in sitemaps)
                {
                    try
                    {
                        var urls = await sitemapLoader.LoadSitemapAsync(sitemap.Url);
                        if (!string.IsNullOrEmpty(sitemap.RegexFilter))
                        {
                            var regex = new Regex(sitemap.RegexFilter);
                            urls = urls.Where(u => regex.IsMatch(u)).ToList();
                        }
                        allUrls.AddRange(urls);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }
            }
            else
            {
                throw new NotImplementedException("Collection for Sku's without sitemaps is not supported.");
            }
            return allUrls;
        }
    }
}