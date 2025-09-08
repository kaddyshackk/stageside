using Microsoft.Playwright;

namespace ComedyPull.Application.Features.DataSync.Interfaces
{
    public interface IPageProcessor
    {
        /// <summary>
        /// Processes a page at the given URL.
        /// </summary>
        /// <param name="url">Url of page to process.</param>
        /// <returns>A <see cref="Task"/> that completes when the page has been processed.</returns>
        public Task ProcessPageAsync(string url, IPage page, CancellationToken cancellationToken);
    }
}
