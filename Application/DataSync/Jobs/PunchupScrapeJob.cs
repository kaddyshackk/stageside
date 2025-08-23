using ComedyPull.Application.DataSync.Interfaces;
using ComedyPull.Application.DataSync.Processors;
using System.Text.RegularExpressions;

namespace Application.DataSync.Jobs
{
    public class PunchupScrapeJob(
        ISitemapLoader sitemapLoader,
        IScraper scraper
    )
    {
        public async Task ExecuteAsync()
        {
            var sitemapUrl = "https://www.punchup.live/sitemap.xml";
            
            var urls = await sitemapLoader.LoadSitemapAsync(sitemapUrl);

            // Filter Urls
            var regex = new Regex("/^https?:\\/\\/(?:www\\.)?punchup\\.live\\/([^\\/]+)\\/tickets(?:\\/)?$/");
            var mathed = urls.Where(url => regex.IsMatch(url)).ToList();

            // Scrape urls
            await scraper.InitializeAsync();
            await scraper.RunAsync(urls, () => new PunchupTicketsPageProcessor());

            // For each URL: Scrape data, transform it, deduplicate it, save to database
        }
    }
}
