using ComedyPull.Application.DataSync.Interfaces;
using Microsoft.Playwright;

namespace ComedyPull.Application.DataSync.Processors
{
    public class PunchupTicketsPageProcessor : IPageProcessor
    {
        /// <summary>
        /// Processes a page at the given URL.
        /// </summary>
        /// <param name="url">Url of page to process.</param>
        /// <returns></returns>
        public Task ProcessPageAsync(string url, IPage page, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
