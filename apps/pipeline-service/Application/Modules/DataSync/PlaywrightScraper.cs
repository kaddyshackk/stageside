using Microsoft.Playwright;
using System.Threading.Channels;
using ComedyPull.Application.Modules.DataSync.Interfaces;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace ComedyPull.Application.Modules.DataSync
{
    public class PlaywrightScraper : IScraper
    {
        private readonly ILogger<PlaywrightScraper> _logger;
        
        private readonly int _concurrency;
        private readonly SemaphoreSlim _semaphore;
        private readonly ChannelWriter<string> _urlWriter;
        private readonly ChannelReader<string> _urlReader;
        private bool _disposed;

        private IPlaywright? _playwright;
        private IBrowser? _browser;
        private IBrowserContext? _context;

        public PlaywrightScraper(int concurrency, ILogger<PlaywrightScraper> logger)
        {
            _logger = logger;
            _concurrency = concurrency;
            _semaphore = new SemaphoreSlim(_concurrency);
            var queue = Channel.CreateUnbounded<string>();
            _urlWriter = queue.Writer;
            _urlReader = queue.Reader;
            _playwright = null;
            _browser = null;
            _context = null;
        }
        
        public PlaywrightScraper(
            int concurrency,
            ILogger<PlaywrightScraper> logger,
            IPlaywright? playwright = null,
            IBrowser? browser = null,
            IBrowserContext? context = null
        )
        {
            _logger = logger;
            _concurrency = concurrency;
            _semaphore = new SemaphoreSlim(_concurrency);
            var queue = Channel.CreateUnbounded<string>();
            _urlWriter = queue.Writer;
            _urlReader = queue.Reader;

            _playwright = playwright;
            _browser = browser;
            _context = context;
        }

        /// <summary>
        /// Initializes the Playwright scraper with the specified launch options.
        /// </summary>
        /// <param name="options">Browser launch options.</param>
        /// <returns>A <see cref="Task"/> that completes when the <see cref="PlaywrightScraper"/> is initialized.</returns>
        public async Task InitializeAsync(BrowserTypeLaunchOptions? options = null)
        {
            _playwright ??= await Playwright.CreateAsync();

            _browser ??= await _playwright.Chromium.LaunchAsync(options ?? new BrowserTypeLaunchOptions
            {
                Headless = true,
                Args = ["--no-sandbox", "--disable-dev-shm-usage"]
            });

            _context ??= await _browser.NewContextAsync(new BrowserNewContextOptions
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
                ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
            });
            
            _logger.LogInformation("Initialized PlaywrightScraper with {@Options}", options);
        }

        /// <summary>
        /// Starts processing the provided URLs using the specified page processor factory.
        /// </summary>
        /// <typeparam name="TProcessor">The type of the desired processor to scrape with.</typeparam>
        /// <param name="urls">A list of urls to scrape.</param>
        /// <param name="processorFactory">A factory method that returns the desired processor to scrape with.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task that completes when all workers have completed processing the urls.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the <see cref="PlaywrightScraper"/> was not properly initialized.</exception>
        public async Task RunAsync<TProcessor>(IEnumerable<string> urls, Func<TProcessor> processorFactory,
            CancellationToken cancellationToken = default)
            where TProcessor : IPageCollector
        {
            using (LogContext.PushProperty("Concurrency", _concurrency))
            using (LogContext.PushProperty("UrlCount", urls))
            {
                _logger.LogInformation("Starting PlaywrightScraper");
            
                if (_browser == null)
                    throw new InvalidOperationException("Scraper not initialized. Call InitializeAsync first.");
                if (_context == null)
                    throw new InvalidOperationException("Scraper failed to initialize a new context.");

                var workers = Enumerable.Range(0, _concurrency)
                    .Select(_ => WorkAsync(processorFactory(), cancellationToken))
                    .ToArray();

                await EnqueueUrlsAsync(urls);
            
                _logger.LogInformation("Started PlaywrightScraper");

                await Task.WhenAll(workers);
            }
        }

        /// <summary>
        /// Processes URLs from the channel using the specified processor.
        /// </summary>
        /// <typeparam name="TProcessor">The type of the processor to be used to scrape URLs.</typeparam>
        /// <param name="processor">The processor to use to scrape the url.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> that completes when no more URLs are available in the queue.</returns>
        private async Task WorkAsync<TProcessor>(TProcessor processor, CancellationToken cancellationToken)
            where TProcessor : IPageCollector
        {
            while (await _urlReader.WaitToReadAsync(cancellationToken))
            {
                if (!_urlReader.TryRead(out var url)) continue;
                await _semaphore.WaitAsync(cancellationToken);

                IPage page = null!;
                try
                {
                    page = await _context!.NewPageAsync();
                    await processor.CollectPageAsync(url, page, cancellationToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing url ${url}: {ex.Message}");
                }
                finally
                {
                    await page.CloseAsync();
                    _semaphore.Release();
                }
            }
        }

        /// <summary>
        /// Enqueues the provided URLs into the internal channel for processing.
        /// </summary>
        /// <param name="urls">The URLs to enqueue.</param>
        /// <returns>A <see cref="Task"/> that completes when the URLs have been enqueued.</returns>
        private async Task EnqueueUrlsAsync(IEnumerable<string> urls)
        {
            foreach (var url in urls)
            {
                await _urlWriter.WriteAsync(url);
            }

            _urlWriter.Complete();
        }

        /// <summary>
        /// Disposes of the Playwright scraper and releases all resources.
        /// </summary>
        /// <returns>A <see cref="Task"/> that completes when the Playwright context and browser have been released.</returns>
        public async Task DisposeAsync()
        {
            if (_disposed) return;

            if (_context != null)
                await _context.CloseAsync();
            if (_browser != null)
                await _browser.CloseAsync();
            _playwright?.Dispose();
            _semaphore?.Dispose();
            _disposed = true;
        }

        /// <summary>
        /// Releases all resources used by the current instance of the class.
        /// </summary>
        /// <remarks>This method synchronously disposes of resources by awaiting the asynchronous disposal
        /// operation. It should be called when the instance is no longer needed to free up resources
        /// promptly.</remarks>
        public void Dispose()
        {
            DisposeAsync().GetAwaiter().GetResult();
        }
    }
}