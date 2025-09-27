using ComedyPull.Application.Modules.DataSync.Configuration;
using ComedyPull.Application.Modules.DataSync.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace ComedyPull.Application.Tests.Modules.DataSync.Services
{
    [TestClass]
    public class PlaywrightScraperFactoryTests
    {
        [TestMethod, TestCategory("Unit")]
        public void CreateScraper_ReturnsPlaywrightScraperWithCorrectConcurrency()
        {
            // Arrange
            var options = Options.Create(new DataSyncOptions
            {
                PunchupCollection = new PunchupCollectionOptions
                {
                    Concurrency = 3
                }
            });
            var factory = new PlaywrightScraperFactory(options);

            // Act
            var scraper = factory.CreateScraper();

            // Assert
            scraper.Should().NotBeNull();
            scraper.Should().BeOfType<PlaywrightScraper>();
        }

        [TestMethod, TestCategory("Unit")]
        public void CreateScraper_ReturnsNewInstanceEachTime()
        {
            // Arrange
            var options = Options.Create(new DataSyncOptions
            {
                PunchupCollection = new PunchupCollectionOptions
                {
                    Concurrency = 3
                }
            });
            var factory = new PlaywrightScraperFactory(options);

            // Act
            var scraper1 = factory.CreateScraper();
            var scraper2 = factory.CreateScraper();

            // Assert
            scraper1.Should().NotBeSameAs(scraper2);
        }
    }
}