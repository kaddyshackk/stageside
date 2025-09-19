using ComedyPull.Application.Features.DataSync.Punchup;
using ComedyPull.Application.Interfaces;
using ComedyPull.Domain.Models;
using FakeItEasy;
using Microsoft.Playwright;

namespace ComedyPull.Application.Tests.Features.DataSync.Punchup
{
    [TestClass]
    public class PunchupTicketsPageProcessorTests
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

        [TestMethod]
        public async Task ProcessPage_ReturnsData()
        {
            // Arrange
            var url = "https://punchup.live/joe-list/tickets";
            var queue = A.Fake<IQueue<BronzeRecord>>();
            var processor = new PunchupTicketsPageProcessor(queue);
            
            // Act
            await processor.ProcessPageAsync(url, _page, CancellationToken.None);

            // Assert
            A.CallTo(() => queue.EnqueueAsync(A<BronzeRecord>._, A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }
    }
}