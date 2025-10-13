using System.Text.Json;
using ComedyPull.Application.Modules.Punchup.Models;
using ComedyPull.Application.Modules.Punchup.Pages;
using ComedyPull.Application.Interfaces;
using ComedyPull.Application.Modules.DataSync.Interfaces;
using ComedyPull.Application.Utils;
using ComedyPull.Domain.Modules.DataProcessing;
using Microsoft.Playwright;

namespace ComedyPull.Application.Modules.Punchup.Collectors
{
    /// <summary>
    /// IPageProcessor implementation that scrapes and stores the result data.
    /// </summary>
    public class PunchupTicketsPageCollector(IQueue<BronzeRecord> queue, string batchId) : IPageCollector
    {
        /// <summary>
        /// Processes a page at the given URL.
        /// </summary>
        /// <param name="url">Url of page to process.</param>
        /// <param name="page">The page to use in processing.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> that completes when processing is completed.</returns>
        public async Task CollectPageAsync(string url, IPage page, CancellationToken cancellationToken)
        {
            var pom = new TicketsPage(page);

            // Load page
            await page.GotoAsync(url);
            await pom.BioSection.WaitForAsync();

            // Parse bio
            var name = await pom.Name.InnerTextAsync();
            var bio = await pom.Bio.InnerTextAsync();

            // Parse shows if available
            var noShows = await pom.NoShowsMessage.IsVisibleAsync();
            var shows = new List<PunchupEvent>();
            if (!noShows)
            {
                if (await pom.SeeAllButton.IsVisibleAsync())
                    await pom.SeeAllButton.WaitForAsync();
                var showLocators = await pom.Show.AllAsync();
                var tasks = showLocators.Select(ProcessShowAsync);
                var results = await Task.WhenAll(tasks);
                shows = results.Where(show => show != null).ToList()!;
            }

            var record = new BronzeRecord
            {
                BatchId = batchId,
                Data = JsonSerializer.Serialize(new PunchupRecord
                {
                    Name = name,
                    Bio = bio,
                    Events = shows,
                }),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System",
            };

            await queue.EnqueueAsync(record, cancellationToken);
        }

        private async Task<PunchupEvent?> ProcessShowAsync(ILocator showLocator)
        {
            var pom = new ShowCard(showLocator);

            // Load data from page
            var ticketLink = await pom.TicketsButton.GetAttributeAsync("href");
            var results = await Task.WhenAll(
                new List<Task<string>>
                {
                    pom.StartDate.InnerTextAsync(),
                    pom.StartTime.InnerTextAsync(),
                    pom.Location.InnerTextAsync(),
                    pom.Venue.InnerTextAsync()
                }
            );

            // Parse time to DateTimeOffset
            var startDateTime = ParseUtils.ParseFutureEventDateTime(results[0], results[1]);
            if (!startDateTime.HasValue)
            {
                throw new InvalidDataException("Could not parse DateTimeOffset from event data.");
            }

            return new PunchupEvent
            {
                StartDateTime = startDateTime.Value,
                Location = results[2],
                Venue = results[3],
                TicketLink = ticketLink
            };
        }
    }
}