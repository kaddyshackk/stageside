using ComedyPull.Application.DataSync.Interfaces;
using Microsoft.Playwright;

namespace ComedyPull.Application.DataSync.Processors
{
    /// <summary>
    /// IPageProcessor implementation that scrapes and stores the result data.
    /// </summary>
    public class PunchupTicketsPageProcessor : IPageProcessor
    {
        /// <summary>
        /// Processes a page at the given URL.
        /// </summary>
        /// <param name="url">Url of page to process.</param>
        /// <param name="page">The page to use in processing.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> that completes when processing is completed.</returns>
        public Task ProcessPageAsync(string url, IPage page, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
