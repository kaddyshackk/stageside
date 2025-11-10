using System.Text.Json;
using StageSide.Pipeline.Domain.Pipeline.Interfaces;
using StageSide.Pipeline.Domain.Pipeline.Models;
using StageSide.Pipeline.Domain.Queue.Interfaces;
using StageSide.Pipeline.Domain.Queue.Models;
using Microsoft.Extensions.Options;
using Serilog.Context;

namespace StageSide.Pipeline.Service.Pipeline.Collection
{
    public class DynamicCollectionService(
        IQueueClient queueClient,
        IBackPressureManager backPressureManager,
        ICollectorFactory collectorFactory,
        IWebBrowser webBrowser,
        IOptions<DynamicCollectionOptions> options,
        ILogger<DynamicCollectionService> logger
        ) : BackgroundService
    {
        private readonly SemaphoreSlim _semaphore = new(options.Value.WebBrowserConcurrency);
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
            
            var workers = Enumerable.Range(0, options.Value.WebBrowserConcurrency)
                .Select(_ => WorkerAsync(stoppingToken))
                .ToArray();

            logger.LogInformation("Starting PlaywrightScraper workers with concurrency {Concurrency}", options.Value.WebBrowserConcurrency);

            await Task.WhenAll(workers);
        }

        private async Task WorkerAsync(CancellationToken stoppingToken)
        {
            using (LogContext.PushProperty("ServiceName", nameof(DynamicCollectionService)))
            {
                logger.LogInformation("Started {ServiceName}", nameof(DynamicCollectionService));
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        if (await backPressureManager.ShouldApplyBackPressureAsync(Queues.Transformation))
                        {
                            logger.LogWarning("Transformation queue overloaded, delaying for another interval.");
                            continue;
                        }
                        if (await queueClient.GetLengthAsync(Queues.DynamicCollection) == 0) continue;

                        var context = await queueClient.DequeueAsync(Queues.DynamicCollection);
                        if (context == null) continue;
                        
                        using (LogContext.PushProperty("ContextId", context.Id))
                        using (LogContext.PushProperty("ExecutionId", context.JobExecutionId))
                        using (LogContext.PushProperty("Sku", context.Sku))
                        using (LogContext.PushProperty("CollectionUrl", context.Metadata.CollectionUrl))
                        using (LogContext.PushProperty("Tags", context.Metadata.Tags))
                        {
                            await _semaphore.WaitAsync(stoppingToken);
                            IWebPage page = null!;
                            try
                            {
                                page = await _context!.NewPageAsync();

                                var collector = collectorFactory.GetPageCollector(context.Sku);
                                if (collector == null)
                                {
                                    logger.LogWarning($"Found no collector that matches content sku {context.Sku}");
                                    context.State = ProcessingState.Failed;
                                    continue;
                                }

                                var result = await collector.CollectPageAsync(context.Metadata.CollectionUrl, page);

                                context.RawData = JsonSerializer.Serialize(result);
                                context.Metadata.CollectedAt = DateTimeOffset.UtcNow;
                                context.State = ProcessingState.Collected;

                                await queueClient.EnqueueAsync(Queues.Transformation, context);
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, "Error processing url {Url} from job execution {BatchId}",
                                    context.Metadata.CollectionUrl,
                                    context.JobExecutionId);
                                context.State = ProcessingState.Failed;
                            }
                            finally
                            {
                                await page.CloseAsync();
                                _semaphore.Release();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error in dynamic collection worker");
                    }
                    finally
                    {
                        var delaySeconds = await backPressureManager.CalculateAdaptiveDelayAsync(Queues.Transformation,
                            options.Value.DelayIntervalSeconds);
                        await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
                    }
                }
                logger.LogInformation("Stopped {ServiceName}", nameof(DynamicCollectionService));
            }
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