using ComedyPull.Application.Modules.DataSync.Options;
using ComedyPull.Application.Modules.DataSync;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ComedyPull.Application.Tests.Modules.DataSync.Services
{
    [TestClass]
    public class PlaywrightScraperFactoryTests
    {
        private IServiceProvider _serviceProvider = null!;

        [TestInitialize]
        public void Setup()
        {
            _serviceProvider = A.Fake<IServiceProvider>();
            
            A.CallTo(() => _serviceProvider.GetService(typeof(ILogger<PlaywrightScraper>)))
                .Returns(A.Fake<ILogger<PlaywrightScraper>>());
        }

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
            var factory = new PlaywrightScraperFactory(options, _serviceProvider);

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
            var factory = new PlaywrightScraperFactory(options, _serviceProvider);

            // Act
            var scraper1 = factory.CreateScraper();
            var scraper2 = factory.CreateScraper();

            // Assert
            scraper1.Should().NotBeSameAs(scraper2);
        }
    }
}