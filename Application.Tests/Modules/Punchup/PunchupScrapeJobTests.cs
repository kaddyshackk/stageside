using ComedyPull.Application.Modules.DataProcessing.Interfaces;
using ComedyPull.Application.Modules.DataSync.Interfaces;
using ComedyPull.Application.Modules.Punchup;
using ComedyPull.Application.Modules.Punchup.Collectors;
using ComedyPull.Application.Modules.Punchup.Collectors.Interfaces;
using ComedyPull.Domain.Enums;
using ComedyPull.Domain.Modules.DataProcessing;
using FakeItEasy;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Quartz;

namespace ComedyPull.Application.Tests.Modules.Punchup
{
    [TestClass]
    public class PunchupScrapeJobTests
    {
        private ISitemapLoader _mockSitemapLoader = null!;
        private IPlaywrightScraperFactory _mockScraperFactory = null!;
        private IPunchupTicketsPageCollectorFactory _mockCollectorFactory = null!;
        private IBatchRepository _mockBatchRepository = null!;
        private IScraper _mockScraper = null!;
        private IMediator _mediator = null!;
        private ILogger<PunchupScrapeJob> _logger = null!;
        private PunchupScrapeJob _job = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockSitemapLoader = A.Fake<ISitemapLoader>();
            _mockScraper = A.Fake<IScraper>();
            _mockScraperFactory = A.Fake<IPlaywrightScraperFactory>();
            _mockCollectorFactory = A.Fake<IPunchupTicketsPageCollectorFactory>();
            _mockBatchRepository = A.Fake<IBatchRepository>();
            _mediator = A.Fake<IMediator>();
            _logger =  A.Fake<ILogger<PunchupScrapeJob>>();

            A.CallTo(() => _mockScraperFactory.CreateScraper()).Returns(_mockScraper);
            
            var mockBatch = new Batch
            {
                Source = DataSource.Punchup,
                SourceType = DataSourceType.PunchupTicketsPage,
                State = ProcessingState.Ingested,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = nameof(PunchupScrapeJob),
                UpdatedAt = DateTimeOffset.UtcNow,
                UpdatedBy = nameof(PunchupScrapeJob)
            };
            A.CallTo(() => _mockBatchRepository.CreateBatch(A<DataSource>._, A<DataSourceType>._, A<string>._, A<CancellationToken>._))
                .Returns(mockBatch);

            _job = new PunchupScrapeJob(_mockSitemapLoader, _mockScraperFactory, _mockCollectorFactory, _mockBatchRepository, _mediator, _logger);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ExecuteAsync_LoadsSitemapFromCorrectUrl()
        {
            const string expectedSitemapUrl = "https://www.punchup.live/sitemap.xml";
            A.CallTo(() => _mockSitemapLoader.LoadSitemapAsync(A<string>._))
                .Returns(new List<string>());

            await _job.Execute(A.Fake<IJobExecutionContext>());

            A.CallTo(() => _mockSitemapLoader.LoadSitemapAsync(expectedSitemapUrl))
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod, TestCategory("Unit")]
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

            await _job.Execute(A.Fake<IJobExecutionContext>());

            object?[] expectedUrls =
            [
                "https://www.punchup.live/comedian1/tickets",
                "https://www.punchup.live/comedian2/tickets/",
                "https://punchup.live/comedian3/tickets"
            ];

            A.CallTo(() => _mockScraper.RunAsync(
                A<IEnumerable<string>>.That.IsSameSequenceAs(expectedUrls),
                A<Func<PunchupTicketsPageCollector>>._,
                A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ExecuteAsync_CreatesScraper()
        {
            A.CallTo(() => _mockSitemapLoader.LoadSitemapAsync(A<string>._))
                .Returns(["https://www.punchup.live/comedian1/tickets"]);

            await _job.Execute(A.Fake<IJobExecutionContext>());

            A.CallTo(() => _mockScraperFactory.CreateScraper())
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ExecuteAsync_InitializesScraperBeforeRunning()
        {
            A.CallTo(() => _mockSitemapLoader.LoadSitemapAsync(A<string>._))
                .Returns(["https://www.punchup.live/comedian1/tickets"]);

            await _job.Execute(A.Fake<IJobExecutionContext>());

            A.CallTo(() => _mockScraperFactory.CreateScraper())
                .MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => _mockScraper.InitializeAsync(null))
                    .MustHaveHappenedOnceExactly())
                .Then(A.CallTo(() => _mockScraper.RunAsync(A<IEnumerable<string>>._, A<Func<PunchupTicketsPageCollector>>._, A<CancellationToken>._))
                    .MustHaveHappenedOnceExactly());
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ExecuteAsync_RunsScraperWithCorrectProcessor()
        {
            var matchedUrls = new List<string> { "https://www.punchup.live/comedian1/tickets" };
            A.CallTo(() => _mockSitemapLoader.LoadSitemapAsync(A<string>._))
                .Returns(matchedUrls);

            await _job.Execute(A.Fake<IJobExecutionContext>());

            A.CallTo(() => _mockScraper.RunAsync(
                A<IEnumerable<string>>.That.Contains("https://www.punchup.live/comedian1/tickets"),
                A<Func<PunchupTicketsPageCollector>>.That.Not.IsNull(),
                A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ExecuteAsync_DisposesScraperAfterExecution()
        {
            A.CallTo(() => _mockSitemapLoader.LoadSitemapAsync(A<string>._))
                .Returns(["https://www.punchup.live/comedian1/tickets"]);

            await _job.Execute(A.Fake<IJobExecutionContext>());

            A.CallTo(() => _mockScraper.RunAsync(A<IEnumerable<string>>._, A<Func<PunchupTicketsPageCollector>>._, A<CancellationToken>._))
                .MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => _mockScraper.Dispose())
                    .MustHaveHappenedOnceExactly());
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ExecuteAsync_WithEmptySitemap_DoesNotCallScraper()
        {
            A.CallTo(() => _mockSitemapLoader.LoadSitemapAsync(A<string>._))
                .Returns([]);

            await _job.Execute(A.Fake<IJobExecutionContext>());

            A.CallTo(() => _mockScraper.RunAsync(A<IEnumerable<string>>._, A<Func<PunchupTicketsPageCollector>>._, A<CancellationToken>._))
                .MustNotHaveHappened();

            A.CallTo(() => _mockScraper.Dispose())
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod, TestCategory("Unit")]
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

            await _job.Execute(A.Fake<IJobExecutionContext>());

            A.CallTo(() => _mockScraper.RunAsync(A<IEnumerable<string>>._, A<Func<PunchupTicketsPageCollector>>._, A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ExecuteAsync_WhenSitemapLoaderThrows_CatchesAndLogsException()
        {
            var expectedException = new HttpRequestException("Network error");
            A.CallTo(() => _mockSitemapLoader.LoadSitemapAsync(A<string>._))
                .Throws(expectedException);

            await _job.Execute(A.Fake<IJobExecutionContext>());

            A.CallTo(_logger)
                .Where(call => call.Method.Name == "Log" &&
                              call.Arguments.Get<LogLevel>(0) == LogLevel.Error &&
                              call.Arguments.Get<Exception?>(3) == expectedException)
                .MustHaveHappened(2, Times.Exactly);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ExecuteAsync_WhenScraperInitializationFails_CatchesAndLogsException()
        {
            var expectedException = new InvalidOperationException("Browser initialization failed");
            A.CallTo(() => _mockSitemapLoader.LoadSitemapAsync(A<string>._))
                .Returns(["https://www.punchup.live/comedian1/tickets"]);
            A.CallTo(() => _mockScraper.InitializeAsync(A<Microsoft.Playwright.BrowserTypeLaunchOptions>._))
                .Throws(expectedException);

            await _job.Execute(A.Fake<IJobExecutionContext>());

            A.CallTo(() => _mockScraper.Dispose())
                .MustHaveHappenedOnceExactly();

            A.CallTo(_logger)
                .Where(call => call.Method.Name == "Log" &&
                              call.Arguments.Get<LogLevel>(0) == LogLevel.Error &&
                              call.Arguments.Get<Exception?>(3) == expectedException)
                .MustHaveHappened(2, Times.Exactly);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ExecuteAsync_WhenScraperRunFails_CatchesAndLogsException()
        {
            var expectedException = new Exception("Scraping failed");
            A.CallTo(() => _mockSitemapLoader.LoadSitemapAsync(A<string>._))
                .Returns(["https://www.punchup.live/comedian1/tickets"]);
            A.CallTo(() => _mockScraper.RunAsync(A<IEnumerable<string>>._, A<Func<PunchupTicketsPageCollector>>._, A<CancellationToken>._))
                .Throws(expectedException);

            await _job.Execute(A.Fake<IJobExecutionContext>());

            A.CallTo(_logger)
                .Where(call => call.Method.Name == "Log" &&
                              call.Arguments.Get<LogLevel>(0) == LogLevel.Error &&
                              call.Arguments.Get<Exception?>(3) == expectedException)
                .MustHaveHappened(2, Times.Exactly);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ExecuteAsync_WhenScraperRunFails_StillDisposesScraperInFinally()
        {
            var expectedException = new Exception("Scraping failed");
            A.CallTo(() => _mockSitemapLoader.LoadSitemapAsync(A<string>._))
                .Returns(["https://www.punchup.live/comedian1/tickets"]);
            A.CallTo(() => _mockScraper.RunAsync(A<IEnumerable<string>>._, A<Func<PunchupTicketsPageCollector>>._, A<CancellationToken>._))
                .Throws(expectedException);

            await _job.Execute(A.Fake<IJobExecutionContext>());

            A.CallTo(() => _mockScraper.Dispose()).MustHaveHappenedOnceExactly();
            A.CallTo(_logger)
                .Where(call => call.Method.Name == "Log" &&
                              call.Arguments.Get<LogLevel>(0) == LogLevel.Error &&
                              call.Arguments.Get<Exception?>(3) == expectedException)
                .MustHaveHappened(2, Times.Exactly);
        }

        [TestMethod, TestCategory("Unit")]
        public void TicketsPageUrlRegex_MatchesValidTicketsUrls()
        {
            var validUrls = new[]
            {
                "https://www.punchup.live/comedian-name/tickets",
                "https://www.punchup.live/comedian-name/tickets/",
                "https://www.punchup.live/comedian-name/tickets",
                "https://punchup.live/comedian-name/tickets",
                "https://punchup.live/comedian-name/tickets/"
            };

            foreach (var url in validUrls)
            {
                var regex = PunchupScrapeJob.GetTicketsPageUrlRegexForTesting();
                regex.IsMatch(url).Should().BeTrue($"URL '{url}' should match the regex pattern");
            }
        }

        [TestMethod, TestCategory("Unit")]
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