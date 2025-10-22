using ComedyPull.Application.Modules.DataSync.Interfaces;
using ComedyPull.Application.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ComedyPull.Application.Modules.DataSync
{
    public class PlaywrightScraperFactory(IOptions<PunchupOptions> options, IServiceProvider serviceProvider) : IPlaywrightScraperFactory
    {
        public IScraper CreateScraper()
        {
            var logger = serviceProvider.GetService(typeof(ILogger<PlaywrightScraper>))
                as ILogger<PlaywrightScraper> ?? throw new InvalidOperationException("Cannot locate service that matches ILogger<PlaywrightScraper>.");
            return new PlaywrightScraper(
                concurrency: options.Value.Concurrency,
                logger: logger
            );
        }
    }
}