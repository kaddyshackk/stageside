using Microsoft.Playwright;

namespace ComedyPull.Application.Features.DataSync.Interfaces
{
    public interface IScraper : IDisposable
    {
        /// <summary>
        /// Initializes the <see cref="IScraper"/> with the specified launch options.
        /// </summary>
        /// <param name="options">Browser launch options.</param>
        /// <returns>A <see cref="Task"/> that completes when the <see cref="IScraper"/> is initialized.</returns>
        public Task InitializeAsync(BrowserTypeLaunchOptions? options = null);

        /// <summary>
        /// Starts processing the provided URLs using the specified page processor factory.
        /// </summary>
        /// <typeparam name="TProcessor">The type of the desired processor to scrape with.</typeparam>
        /// <param name="urls">A list of urls to scrape.</param>
        /// <param name="processorFactory">A factory method that returns the desired processor to scrape with.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task that complets when all workers have completed processing the urls.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the <see cref="IScraper"/> was not properly initialized.</exception>
        public Task RunAsync<TProcessor>(IEnumerable<string> urls, Func<TProcessor> processorFactory,
            CancellationToken cancellationToken = default)
            where TProcessor : IPageProcessor;
    }
}