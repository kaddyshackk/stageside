using StageSide.Collection.WebBrowser;
using StageSide.Collection.WebBrowser.Interfaces;

namespace StageSide.Pipeline.Domain.Sources.Punchup.Pages
{
    public class TicketsPage
    {
        // Global
        public readonly IWebElement SeeAllButton;
        public readonly IWebElement NoShowsMessage;

        // Bio
        public readonly IWebElement BioSection;
        public readonly IWebElement Name;
        public readonly IWebElement Bio;

        // Shows
        public readonly IWebElement Show;

        public TicketsPage(IWebPage page)
        {
            // Global
            SeeAllButton = page.GetByRole("button", new WebElementParams
            {
                Name = "See all shows"
            });
            NoShowsMessage = page.GetByText("No shows scheduled");
            // Bio
            BioSection = page.Locator("section").Nth(-1);
            Name = BioSection.Locator("h3");
            Bio = BioSection.Locator("div[class^=\"order-4\"]");
            // Shows
            var showSection = page.Locator("section").Nth(-2);
            Show = showSection.Locator("div[class^=\"w-full mb-4\"]");
        }
    }
}