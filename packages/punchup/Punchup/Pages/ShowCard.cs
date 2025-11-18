using StageSide.Collection.WebBrowser.Interfaces;

namespace StageSide.Punchup.Pages
{
    public class ShowCard(IWebElement locator)
    {
        public readonly IWebElement StartDate = locator.Locator("h4.mb-1.text-xl.xs\\:text-2xl.font-bold");
        public readonly IWebElement Location = locator.Locator("h6.text-xl.xs\\:text-2xl");
        public readonly IWebElement StartTime = locator.Locator("div.flex.flex-col.justify-end p").First;
        public readonly IWebElement Venue = locator.Locator("div.flex.flex-col.justify-end p").Nth(1);
        public readonly IWebElement TicketsButton = locator.Locator("a.flex.justify-center");
    }
}