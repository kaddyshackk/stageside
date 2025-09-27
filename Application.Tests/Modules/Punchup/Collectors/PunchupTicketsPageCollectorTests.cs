using ComedyPull.Application.Interfaces;
using ComedyPull.Application.Modules.Punchup.Collectors;
using ComedyPull.Domain.Models.Processing;
using FakeItEasy;
using Microsoft.Playwright;

namespace ComedyPull.Application.Tests.Modules.Punchup.Collectors
{
    [TestClass]
    public class PunchupTicketsPageCollectorTests
    {
        private static IPlaywright _playwright = null!;
        private static IBrowser _browser = null!;
        private IPage _page = null!;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _playwright = Playwright.CreateAsync().Result;
            _browser = _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false
            }).Result;
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            _browser.DisposeAsync().GetAwaiter().GetResult();
            _playwright?.Dispose();
        }

        [TestInitialize]
        public async Task TestInitialize()
        {
            _page = await _browser.NewPageAsync();
        }

        [TestCleanup]
        public async Task TestCleanup()
        {
            await _page.CloseAsync();
        }

        [TestMethod, TestCategory("Playwright")]
        public async Task ProcessPage_ReturnsData()
        {
            // Arrange
            var url = "https://punchup.live/joe-list/tickets";
            var queue = A.Fake<IQueue<SourceRecord>>();
            var processor = new PunchupTicketsPageCollector(queue);

            // Act
            await processor.CollectPageAsync(url, _page, CancellationToken.None);

            // Assert
            A.CallTo(() => queue.EnqueueAsync(A<SourceRecord>._, A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }
    }
}