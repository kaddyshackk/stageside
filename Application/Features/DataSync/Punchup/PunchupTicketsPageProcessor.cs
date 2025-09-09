using ComedyPull.Application.Features.DataSync.Interfaces;
using ComedyPull.Application.Features.DataSync.Punchup.Models;
using ComedyPull.Application.Features.DataSync.Punchup.Pages;
using ComedyPull.Application.Utils;
using Microsoft.Playwright;

namespace ComedyPull.Application.Features.DataSync.Punchup
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
        public async Task ProcessPageAsync(string url, IPage page, CancellationToken cancellationToken)
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
                await pom.SeeAllButton.WaitForAsync();
                var showLocators = await pom.Show.AllAsync();
                var tasks = showLocators.Select(ProcessShowAsync);
                var results = await Task.WhenAll(tasks);
                shows = results.Where(show => show != null).ToList()!;
            }

            var data = new PunchupRecord
            {
                Name = name,
                Bio = bio,
                Events = shows,
            };
            
            Console.WriteLine(data);
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
