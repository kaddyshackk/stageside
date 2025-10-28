using ComedyPull.Application.Options;
using ComedyPull.Domain.Interfaces.Factory;
using ComedyPull.Domain.Interfaces.Processing;
using ComedyPull.Domain.Interfaces.Service;
using ComedyPull.Domain.Models.Pipeline;
using ComedyPull.Domain.Models.Queue;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ComedyPull.Application.Services
{
    public class DynamicCollectionService(
        IQueueClient queueClient,
        ICollectorFactory collectorFactory,
        IWebBrowser webBrowser,
        IOptions<DataPipelineOptions> options,
        ILogger<DynamicCollectionService> logger
        ) : BackgroundService
    {
        private readonly SemaphoreSlim _semaphore = new(options.Value.Concurrency);
        private bool _disposed;

        private IWebBrowserInstance? _browser;
        private IWebBrowserContext? _context;

        /// <summary>
        /// Initializes the browser with the specified launch options.
        /// </summary>
        /// <param name="launchOptions">Browser launch options.</param>
        /// <returns>A <see cref="Task"/> that completes when the <see cref="DynamicCollectionService"/> is initialized.</returns>
        private async Task InitializeAsync(WebBrowserLaunchOptions? launchOptions = null)
        {
            _browser ??= await webBrowser.Chromium.LaunchAsync(launchOptions ?? new WebBrowserLaunchOptions
            {
                Headless = true,
                Args = ["--no-sandbox", "--disable-dev-shm-usage"]
            });

            _context ??= await _browser.NewContextAsync(new WebBrowserContextOptions
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
                ViewportSize = new WebViewportSize { Width = 1920, Height = 1080 }
            });
            
            logger.LogInformation("Initialized browser with {@Options}", launchOptions);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await InitializeAsync();
            
            logger.LogInformation("Starting PlaywrightScraper workers with concurrency {Concurrency}", options.Value.Concurrency);
            
            var workers = Enumerable.Range(0, options.Value.Concurrency)
                .Select(_ => WorkerAsync(stoppingToken))
                .ToArray();

            await Task.WhenAll(workers);
        }

        private async Task WorkerAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Starting {Service}", nameof(DynamicCollectionService));
            
            while (!stoppingToken.IsCancellationRequested)
            {
                if (await queueClient.GetLengthAsync(Queues.DynamicCollection) > 0)
                {
                    var context = await queueClient.DequeueAsync(Queues.DynamicCollection);
                    if (context == null)
                    {
                        continue;
                    }

                    await _semaphore.WaitAsync(stoppingToken);

                    IWebPage page = null!;
                    try
                    {
                        page = await _context!.NewPageAsync();

                        var collector = collectorFactory.GetPageCollector(context.Sku);
                        if (collector == null)
                        {
                            logger.LogWarning("Found no collector that matches content sku {Sku}", context.Sku);
                            continue;
                        }

                        var result = await collector.CollectPageAsync(context.Metadata.CollectionUrl, page);

                        context.RawData = result;
                        context.Metadata.CollectedAt = DateTimeOffset.UtcNow;
                        context.State = ProcessingState.Collected;
                        
                        await queueClient.EnqueueAsync(Queues.Transformation, context);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error processing url {Url} from job execution {BatchId}", context.Metadata.CollectionUrl,
                            context.JobExecutionId);
                    }
                    finally
                    {
                        await page.CloseAsync();
                        _semaphore.Release();
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
            
            logger.LogInformation("Stopping {Service}", nameof(DynamicCollectionService));
        }

        /// <summary>
        /// Disposes of the browser and releases all resources.
        /// </summary>
        /// <returns>A <see cref="Task"/> that completes when the browser context and browser have been released.</returns>
        private async Task DisposeAsync()
        {
            if (_disposed) return;

            if (_context != null)
                await _context.CloseAsync();
            if (_browser != null)
                await _browser.CloseAsync();
            webBrowser?.Dispose();
            _semaphore?.Dispose();
            _disposed = true;
        }

        /// <summary>
        /// Releases all resources used by the current instance of the class.
        /// </summary>
        /// <remarks>This method synchronously disposes of resources by awaiting the asynchronous disposal
        /// operation. It should be called when the instance is no longer needed to free up resources
        /// promptly.</remarks>
        public override void Dispose()
        {
            DisposeAsync().GetAwaiter().GetResult();
        }
    }
}