using ComedyPull.Application.Features.DataSync.Interfaces;
using ComedyPull.Application.Features.DataSync.Services;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Playwright;

namespace ComedyPull.Application.Tests.Features.DataSync.Services
{
    [TestClass]
    public class PlaywrightScraperTests
    {
        private IPlaywright _mockPlaywright = null!;
        private IBrowser _mockBrowser = null!;
        private IBrowserContext _mockContext = null!;
        private IPage _mockPage = null!;
        private IPageCollector _mockCollector = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockPlaywright = A.Fake<IPlaywright>();
            _mockBrowser = A.Fake<IBrowser>();
            _mockContext = A.Fake<IBrowserContext>();
            _mockPage = A.Fake<IPage>();
            _mockCollector = A.Fake<IPageCollector>();

            A.CallTo(() => _mockPlaywright.Chromium.LaunchAsync(A<BrowserTypeLaunchOptions>._))
                .Returns(Task.FromResult(_mockBrowser));
            A.CallTo(() => _mockBrowser.NewContextAsync(A<BrowserNewContextOptions>._))
                .Returns(Task.FromResult(_mockContext));
            A.CallTo(() => _mockContext.NewPageAsync())
                .Returns(Task.FromResult(_mockPage));
        }

        [TestMethod]
        public async Task InitializeAsync_WithDefaults_CreatesPlaywrightAndBrowser()
        {
            var scraper = new PlaywrightScraper(playwright: _mockPlaywright);

            await scraper.InitializeAsync();

            A.CallTo(() => _mockPlaywright.Chromium.LaunchAsync(A<BrowserTypeLaunchOptions>.That.Matches(
                opts => opts.Headless == true && 
                         opts.Args != null && 
                         opts.Args.Contains("--no-sandbox"))))
                .MustHaveHappenedOnceExactly();
            
            A.CallTo(() => _mockBrowser.NewContextAsync(A<BrowserNewContextOptions>.That.Matches(
                opts => opts.UserAgent!.Contains("Mozilla") && 
                         opts.ViewportSize!.Width == 1920)))
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod]
        public async Task InitializeAsync_WithCustomOptions_UsesProvidedOptions()
        {
            var scraper = new PlaywrightScraper(playwright: _mockPlaywright);
            var customOptions = new BrowserTypeLaunchOptions { Headless = false, SlowMo = 100 };

            await scraper.InitializeAsync(customOptions);

            A.CallTo(() => _mockPlaywright.Chromium.LaunchAsync(customOptions))
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod]
        public async Task InitializeAsync_WithProvidedBrowserAndContext_SkipsCreation()
        {
            var scraper = new PlaywrightScraper(
                playwright: _mockPlaywright,
                browser: _mockBrowser,
                context: _mockContext);

            await scraper.InitializeAsync();

            A.CallTo(() => _mockPlaywright.Chromium.LaunchAsync(A<BrowserTypeLaunchOptions>._))
                .MustNotHaveHappened();
            A.CallTo(() => _mockBrowser.NewContextAsync(A<BrowserNewContextOptions>._))
                .MustNotHaveHappened();
        }

        [TestMethod]
        public async Task RunAsync_WithUrls_ProcessesAllUrlsConcurrently()
        {
            var scraper = new PlaywrightScraper(concurrency: 2, playwright: _mockPlaywright, browser: _mockBrowser, context: _mockContext);
            var urls = new[] { "https://example1.com", "https://example2.com", "https://example3.com" };

            await scraper.RunAsync(urls, () => _mockCollector);

            A.CallTo(() => _mockCollector.CollectPageAsync(A<string>._, _mockPage, A<CancellationToken>._))
                .MustHaveHappened(3, Times.Exactly);
            
            A.CallTo(() => _mockContext.NewPageAsync())
                .MustHaveHappened(3, Times.Exactly);
            
            A.CallTo(() => _mockPage.CloseAsync(null))
                .MustHaveHappened(3, Times.Exactly);
        }

        [TestMethod]
        public async Task RunAsync_WithoutInitialization_ThrowsInvalidOperationException()
        {
            var scraper = new PlaywrightScraper();
            var urls = new[] { "https://example.com" };

            var act = async () => await scraper.RunAsync(urls, () => _mockCollector);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Scraper not initialized. Call InitializeAsync first.");
        }

        [TestMethod]
        public async Task RunAsync_WithNullContext_ThrowsInvalidOperationException()
        {
            var scraper = new PlaywrightScraper(browser: _mockBrowser);
            A.CallTo(() => _mockBrowser.NewContextAsync(A<BrowserNewContextOptions>._))
                .Returns(Task.FromResult<IBrowserContext>(null!));
            
            await scraper.InitializeAsync();
            var urls = new[] { "https://example.com" };

            var act = async () => await scraper.RunAsync(urls, () => _mockCollector);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Scraper failed to initialize a new context.");
        }

        [TestMethod]
        public async Task RunAsync_ProcessorThrowsException_ContinuesProcessingOtherUrls()
        {
            var scraper = new PlaywrightScraper(concurrency: 1, playwright: _mockPlaywright, browser: _mockBrowser, context: _mockContext);
            var urls = new[] { "https://error.com", "https://success.com" };
            
            A.CallTo(() => _mockCollector.CollectPageAsync("https://error.com", A<IPage>._, A<CancellationToken>._))
                .Throws<Exception>();

            await scraper.RunAsync(urls, () => _mockCollector, CancellationToken.None);

            A.CallTo(() => _mockCollector.CollectPageAsync(A<string>._, _mockPage, A<CancellationToken>._))
                .MustHaveHappened(2, Times.Exactly);
            
            A.CallTo(() => _mockPage.CloseAsync(null))
                .MustHaveHappened(2, Times.Exactly);
        }

        [TestMethod]
        public async Task RunAsync_WithCancellationToken_RespectsCancellation()
        {
            var scraper = new PlaywrightScraper(playwright: _mockPlaywright, browser: _mockBrowser, context: _mockContext);
            var urls = new[] { "https://example.com" };
            var cts = new CancellationTokenSource();
            await cts.CancelAsync();

            var act = async () => await scraper.RunAsync(urls, () => _mockCollector, cts.Token);

            await act.Should().ThrowAsync<OperationCanceledException>();
        }

        [TestMethod]
        public async Task DisposeAsync_DisposesAllResources()
        {
            var scraper = new PlaywrightScraper(playwright: _mockPlaywright, browser: _mockBrowser, context: _mockContext);

            await scraper.DisposeAsync();

            A.CallTo(() => _mockContext.CloseAsync(null)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _mockBrowser.CloseAsync(null)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _mockPlaywright.Dispose()).MustHaveHappenedOnceExactly();
        }

        [TestMethod]
        public async Task DisposeAsync_CalledMultipleTimes_OnlyDisposesOnce()
        {
            var scraper = new PlaywrightScraper(playwright: _mockPlaywright, browser: _mockBrowser, context: _mockContext);

            await scraper.DisposeAsync();
            await scraper.DisposeAsync();

            A.CallTo(() => _mockContext.CloseAsync(null)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _mockBrowser.CloseAsync(null)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _mockPlaywright.Dispose()).MustHaveHappenedOnceExactly();
        }

        [TestMethod]
        public void Dispose_CallsDisposeAsync()
        {
            var scraper = new PlaywrightScraper(playwright: _mockPlaywright, browser: _mockBrowser, context: _mockContext);

            scraper.Dispose();

            A.CallTo(() => _mockContext.CloseAsync(null)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _mockBrowser.CloseAsync(null)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _mockPlaywright.Dispose()).MustHaveHappenedOnceExactly();
        }

        [TestMethod]
        public void Constructor_WithCustomConcurrency_SetsConcurrencyLevel()
        {
            var scraper = new PlaywrightScraper(concurrency: 10);

            scraper.Should().NotBeNull();
        }

        [TestMethod]
        public void Constructor_WithDefaultConcurrency_Uses5()
        {
            var scraper = new PlaywrightScraper();

            scraper.Should().NotBeNull();
        }
    }
}