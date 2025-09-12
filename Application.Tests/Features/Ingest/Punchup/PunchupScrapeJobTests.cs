using ComedyPull.Application.Features.Ingest.Interfaces;
using ComedyPull.Application.Features.Ingest.Punchup;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace ComedyPull.Application.Tests.Features.Ingest.Punchup
{
    [TestClass]
    public class PunchupScrapeJobTests
    {
        private ISitemapLoader _mockSitemapLoader = null!;
        private IScraper _mockScraper = null!;
        private IServiceProvider _serviceProvider = null!;
        private ILogger<PunchupScrapeJob> _logger = null!;
        private PunchupScrapeJob _job = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockSitemapLoader = A.Fake<ISitemapLoader>();
            _mockScraper = A.Fake<IScraper>();
            _serviceProvider = A.Fake<IServiceProvider>();
            _logger =  A.Fake<ILogger<PunchupScrapeJob>>();
            _job = new PunchupScrapeJob(_mockSitemapLoader, _mockScraper, _serviceProvider, _logger);
        }

        [TestMethod]
        public async Task ExecuteAsync_LoadsSitemapFromCorrectUrl()
        {
            const string expectedSitemapUrl = "https://www.punchup.live/sitemap.xml";
            A.CallTo(() => _mockSitemapLoader.LoadSitemapAsync(A<string>._))
                .Returns(new List<string>());

            await _job.ExecuteAsync();

            A.CallTo(() => _mockSitemapLoader.LoadSitemapAsync(expectedSitemapUrl))
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod]
        public async Task ExecuteAsync_FiltersUrlsUsingTicketsPageRegex()
        {
            var sitemapUrls = new List<string>
            {
                "https://www.punchup.live/comedian1/tickets",
                "https://www.punchup.live/comedian2/tickets/",
                "https://punchup.live/comedian3/tickets",
                "https://www.punchup.live/comedian1/bio", // Should be filtered out
                "https://www.punchup.live/comedian2/events", // Should be filtered out
                "https://other-site.com/comedian3/tickets" // Should be filtered out
            };

            A.CallTo(() => _mockSitemapLoader.LoadSitemapAsync(A<string>._))
                .Returns(sitemapUrls);

            await _job.ExecuteAsync();

            object?[] expectedUrls =
            [
                "https://www.punchup.live/comedian1/tickets",
                "https://www.punchup.live/comedian2/tickets/",
                "https://punchup.live/comedian3/tickets"
            ];

            A.CallTo(() => _mockScraper.RunAsync(
                A<IEnumerable<string>>.That.IsSameSequenceAs(expectedUrls),
                A<Func<PunchupTicketsPageProcessor>>._,
                A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod]
        public async Task ExecuteAsync_InitializesScraperBeforeRunning()
        {
            A.CallTo(() => _mockSitemapLoader.LoadSitemapAsync(A<string>._))
                .Returns(["https://www.punchup.live/comedian1/tickets"]);

            await _job.ExecuteAsync();

            A.CallTo(() => _mockScraper.InitializeAsync(null))
                .MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => _mockScraper.RunAsync(A<IEnumerable<string>>._, A<Func<PunchupTicketsPageProcessor>>._, A<CancellationToken>._))
                    .MustHaveHappenedOnceExactly());
        }

        [TestMethod]
        public async Task ExecuteAsync_RunsScraperWithCorrectProcessor()
        {
            var matchedUrls = new List<string> { "https://www.punchup.live/comedian1/tickets" };
            A.CallTo(() => _mockSitemapLoader.LoadSitemapAsync(A<string>._))
                .Returns(matchedUrls);

            await _job.ExecuteAsync();

            A.CallTo(() => _mockScraper.RunAsync(
                A<IEnumerable<string>>.That.Contains("https://www.punchup.live/comedian1/tickets"),
                A<Func<PunchupTicketsPageProcessor>>.That.Not.IsNull(),
                A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod]
        public async Task ExecuteAsync_DisposesScraperAfterExecution()
        {
            A.CallTo(() => _mockSitemapLoader.LoadSitemapAsync(A<string>._))
                .Returns(["https://www.punchup.live/comedian1/tickets"]);

            await _job.ExecuteAsync();

            A.CallTo(() => _mockScraper.RunAsync(A<IEnumerable<string>>._, A<Func<PunchupTicketsPageProcessor>>._, A<CancellationToken>._))
                .MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => _mockScraper.Dispose())
                    .MustHaveHappenedOnceExactly());
        }

        [TestMethod]
        public async Task ExecuteAsync_WithEmptySitemap_DoesNotCallScraper()
        {
            A.CallTo(() => _mockSitemapLoader.LoadSitemapAsync(A<string>._))
                .Returns([]);

            await _job.ExecuteAsync();

            A.CallTo(() => _mockScraper.RunAsync(A<IEnumerable<string>>._, A<Func<PunchupTicketsPageProcessor>>._, A<CancellationToken>._))
                .MustNotHaveHappened();

            A.CallTo(() => _mockScraper.Dispose())
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod]
        public async Task ExecuteAsync_WithNoMatchingUrls_DoesNotCallScraper()
        {
            var sitemapUrls = new List<string>
            {
                "https://www.punchup.live/comedian1/bio",
                "https://www.punchup.live/comedian2/events",
                "https://other-site.com/comedian3/tickets"
            };

            A.CallTo(() => _mockSitemapLoader.LoadSitemapAsync(A<string>._))
                .Returns(sitemapUrls);

            await _job.ExecuteAsync();

            A.CallTo(() => _mockScraper.RunAsync(A<IEnumerable<string>>._, A<Func<PunchupTicketsPageProcessor>>._, A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [TestMethod]
        public async Task ExecuteAsync_WhenSitemapLoaderThrows_CatchesAndLogsException()
        {
            var expectedException = new HttpRequestException("Network error");
            A.CallTo(() => _mockSitemapLoader.LoadSitemapAsync(A<string>._))
                .Throws(expectedException);

            await _job.ExecuteAsync();

            A.CallTo(_logger)
                .Where(call => call.Method.Name == "Log" &&
                              call.Arguments.Get<LogLevel>(0) == LogLevel.Error &&
                              call.Arguments.Get<Exception?>(3) == expectedException)
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod]
        public async Task ExecuteAsync_WhenScraperInitializationFails_CatchesAndLogsException()
        {
            var expectedException = new InvalidOperationException("Browser initialization failed");
            A.CallTo(() => _mockSitemapLoader.LoadSitemapAsync(A<string>._))
                .Returns(["https://www.punchup.live/comedian1/tickets"]);
            A.CallTo(() => _mockScraper.InitializeAsync(A<Microsoft.Playwright.BrowserTypeLaunchOptions>._))
                .Throws(expectedException);

            await _job.ExecuteAsync();

            A.CallTo(() => _mockScraper.Dispose())
                .MustHaveHappenedOnceExactly();
            
            A.CallTo(_logger)
                .Where(call => call.Method.Name == "Log" &&
                              call.Arguments.Get<LogLevel>(0) == LogLevel.Error &&
                              call.Arguments.Get<Exception?>(3) == expectedException)
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod]
        public async Task ExecuteAsync_WhenScraperRunFails_CatchesAndLogsException()
        {
            var expectedException = new Exception("Scraping failed");
            A.CallTo(() => _mockSitemapLoader.LoadSitemapAsync(A<string>._))
                .Returns(["https://www.punchup.live/comedian1/tickets"]);
            A.CallTo(() => _mockScraper.RunAsync(A<IEnumerable<string>>._, A<Func<PunchupTicketsPageProcessor>>._, A<CancellationToken>._))
                .Throws(expectedException);

            await _job.ExecuteAsync();

            A.CallTo(_logger)
                .Where(call => call.Method.Name == "Log" &&
                              call.Arguments.Get<LogLevel>(0) == LogLevel.Error &&
                              call.Arguments.Get<Exception?>(3) == expectedException)
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod]
        public async Task ExecuteAsync_WhenScraperRunFails_StillDisposesScraperInFinally()
        {
            var expectedException = new Exception("Scraping failed");
            A.CallTo(() => _mockSitemapLoader.LoadSitemapAsync(A<string>._))
                .Returns(["https://www.punchup.live/comedian1/tickets"]);
            A.CallTo(() => _mockScraper.RunAsync(A<IEnumerable<string>>._, A<Func<PunchupTicketsPageProcessor>>._, A<CancellationToken>._))
                .Throws(expectedException);

            await _job.ExecuteAsync();

            A.CallTo(() => _mockScraper.Dispose()).MustHaveHappenedOnceExactly();
            A.CallTo(_logger)
                .Where(call => call.Method.Name == "Log" &&
                              call.Arguments.Get<LogLevel>(0) == LogLevel.Error &&
                              call.Arguments.Get<Exception?>(3) == expectedException)
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod]
        public void TicketsPageUrlRegex_MatchesValidTicketsUrls()
        {
            var validUrls = new[]
            {
                "https://www.punchup.live/comedian-name/tickets",
                "https://www.punchup.live/comedian-name/tickets/",
                "http://www.punchup.live/comedian-name/tickets",
                "https://punchup.live/comedian-name/tickets",
                "http://punchup.live/comedian-name/tickets/"
            };

            foreach (var url in validUrls)
            {
                var regex = PunchupScrapeJob.GetTicketsPageUrlRegexForTesting();
                regex.IsMatch(url).Should().BeTrue($"URL '{url}' should match the regex pattern");
            }
        }

        [TestMethod]
        public void TicketsPageUrlRegex_DoesNotMatchInvalidUrls()
        {
            var invalidUrls = new[]
            {
                "https://www.punchup.live/comedian-name/bio",
                "https://www.punchup.live/comedian-name/events",
                "https://other-site.com/comedian-name/tickets",
                "https://www.punchup.live/tickets",
                "https://www.punchup.live/comedian-name",
                "https://www.punchup.live/comedian-name/tickets/extra-path"
            };

            foreach (var url in invalidUrls)
            {
                var regex = PunchupScrapeJob.GetTicketsPageUrlRegexForTesting();
                regex.IsMatch(url).Should().BeFalse($"URL '{url}' should not match the regex pattern");
            }
        }
    }
}