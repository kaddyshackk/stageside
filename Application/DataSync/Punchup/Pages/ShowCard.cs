using Microsoft.Playwright;

namespace ComedyPull.Application.DataSync.Punchup.Pages
{
    public class ShowCard(ILocator locator)
    {
        public readonly ILocator StartDate = locator.Locator("h4.mb-1.text-xl.xs\\:text-2xl.font-bold");
        public readonly ILocator Location = locator.Locator("h6.text-xl.xs\\:text-2xl");
        public readonly ILocator StartTime = locator.Locator("div.flex.flex-col.justify-end p").First;
        public readonly ILocator Venue = locator.Locator("div.flex.flex-col.justify-end p").Nth(1);
        public readonly ILocator TicketsButton = locator.Locator("a.flex.justify-center");
    }
}