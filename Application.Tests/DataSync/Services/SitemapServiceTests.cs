using ComedyPull.Application.DataSync.Services;
using FakeItEasy;
using FluentAssertions;
using RichardSzalay.MockHttp;

namespace ComedyPull.Application.Tests.DataSync.Services
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

        [TestInitialize]
        public void Setup()
        {
            _mockHandler = new MockHttpMessageHandler();
            _httpClient = new HttpClient(_mockHandler);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _httpClient?.Dispose();
            _mockHandler?.Dispose();
        }

        [TestMethod]
        public async Task LoadSitemapAsync_ShouldFetchAndParseSitemap()
        {
            // Arrange
            var sitemapUrl = "https://www.punchup.live/sitemap.xml";
            var sitemapService = new SitemapLoader(_httpClient);

            _mockHandler
                .When(sitemapUrl)
                .Respond("application/xml", SampleSitemap);

            // Act
            var response = await sitemapService.LoadSitemapAsync(sitemapUrl);

            // Assert
            response.Should().NotBeNull();
            response.Should().HaveCount(3);
            response.ElementAt(0).Should().BeOfType<string>(); 
        }
    }
}
