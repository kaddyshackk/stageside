using System.Text.Json;
using ComedyPull.Domain.Interfaces.Processing;
using ComedyPull.Domain.Sources.Punchup.Models;
using ComedyPull.Domain.Sources.Punchup.Pages;
using ComedyPull.Domain.Utils;

namespace ComedyPull.Domain.Sources.Punchup
{
    public class PunchupTicketsPageCollector : IDynamicCollector
    {
        public async Task<string> CollectPageAsync(string url, IWebPage page)
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

            return JsonSerializer.Serialize(new PunchupRecord
            {
                Name = name,
                Bio = bio,
                Events = shows,
            });
        }

        private async Task<PunchupEvent?> ProcessShowAsync(IWebElement showLocator)
        {
            var pom = new ShowCard(showLocator);

            // Load data from page
            var ticketLink = await pom.TicketsButton.GetAttributeAsync("href");
            var results = await Task.WhenAll(
                new List<Task<string?>>
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