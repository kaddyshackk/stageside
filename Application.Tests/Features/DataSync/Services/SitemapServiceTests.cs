using ComedyPull.Application.Features.DataSync.Services;
using FakeItEasy;
using FluentAssertions;
using RichardSzalay.MockHttp;

namespace ComedyPull.Application.Tests.Features.DataSync.Services
{
    [TestClass]
    public class SitemapServiceTests
    {
        private static readonly string SampleSitemap = """
            <?xml version="1.0" encoding="UTF-8"?>
            <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
                <url>
                    <loc>https://example.com/</loc>
                    <lastmod>2024-01-15T10:30:00Z</lastmod>
                    <changefreq>daily</changefreq>
                    <priority>1.0</priority>
                </url>
                <url>
                    <loc>https://example.com/about</loc>
                    <lastmod>2024-01-10T14:20:00Z</lastmod>
                    <changefreq>monthly</changefreq>
                    <priority>0.8</priority>
                </url>
                <url>
                    <loc>https://example.com/products</loc>
                    <lastmod>2024-01-12T09:15:00Z</lastmod>
                    <changefreq>weekly</changefreq>
                    <priority>0.9</priority>
                </url>
            </urlset>
            """;

        private MockHttpMessageHandler _mockHandler = null!;
        private HttpClient _httpClient = null!;
        private IHttpClientFactory _mockHttpClientFactory = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockHandler = new MockHttpMessageHandler();
            _httpClient = new HttpClient(_mockHandler);
            _mockHttpClientFactory = A.Fake<IHttpClientFactory>();
            
            A.CallTo(() => _mockHttpClientFactory.CreateClient(A<string>._))
                .Returns(_httpClient);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _httpClient.Dispose();
            _mockHandler.Dispose();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task LoadSitemapAsync_ShouldFetchAndParseSitemap()
        {
            // Arrange
            const string sitemapUrl = "https://www.punchup.live/sitemap.xml";
            var sitemapService = new SitemapLoader(_mockHttpClientFactory);

            _mockHandler
                .When(sitemapUrl)
                .Respond("application/xml", SampleSitemap);

            // Act
            var response = await sitemapService.LoadSitemapAsync(sitemapUrl);

            // Assert
            response.Should().NotBeNull();
            response.Should().HaveCount(3);
            response.Should().Contain("https://example.com/");
            response.Should().Contain("https://example.com/about");
            response.Should().Contain("https://example.com/products");
        }

        [TestMethod, TestCategory("Unit")]
        public async Task LoadSitemapAsync_CreatesHttpClientFromFactory()
        {
            // Arrange
            var sitemapUrl = "https://www.punchup.live/sitemap.xml";
            var sitemapService = new SitemapLoader(_mockHttpClientFactory);

            _mockHandler
                .When(sitemapUrl)
                .Respond("application/xml", SampleSitemap);

            // Act
            await sitemapService.LoadSitemapAsync(sitemapUrl);

            // Assert
            A.CallTo(() => _mockHttpClientFactory.CreateClient(A<string>._))
                .MustHaveHappenedOnceExactly();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task LoadSitemapAsync_WithInvalidXml_ThrowsException()
        {
            // Arrange
            var sitemapUrl = "https://www.punchup.live/sitemap.xml";
            var sitemapService = new SitemapLoader(_mockHttpClientFactory);

            _mockHandler
                .When(sitemapUrl)
                .Respond("application/xml", "invalid xml content");

            // Act & Assert
            var act = async () => await sitemapService.LoadSitemapAsync(sitemapUrl);
            await act.Should().ThrowAsync<System.Xml.XmlException>();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task LoadSitemapAsync_WithHttpError_ThrowsHttpRequestException()
        {
            // Arrange
            var sitemapUrl = "https://www.punchup.live/sitemap.xml";
            var sitemapService = new SitemapLoader(_mockHttpClientFactory);

            _mockHandler
                .When(sitemapUrl)
                .Respond(System.Net.HttpStatusCode.NotFound);

            // Act & Assert
            var act = async () => await sitemapService.LoadSitemapAsync(sitemapUrl);
            await act.Should().ThrowAsync<HttpRequestException>();
        }
    }
}
