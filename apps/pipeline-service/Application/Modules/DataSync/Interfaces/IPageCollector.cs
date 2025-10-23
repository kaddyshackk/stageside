using Microsoft.Playwright;

namespace ComedyPull.Application.Modules.DataSync.Interfaces
{
    /// <summary>
    /// Represents a service that collects from a specific page using playwright.
    /// </summary>
    public interface IPageCollector
    {
        /// <summary>
        /// Processes a page at the given URL.
        /// </summary>
        /// <param name="url">Url of page to process.</param>
        /// <param name="page">Page object to use in collection.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A <see cref="Task"/> that completes when the page has been processed.</returns>
        public Task CollectPageAsync(string url, IPage page, CancellationToken cancellationToken);
    }
}