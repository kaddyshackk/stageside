using Microsoft.Playwright;

namespace ComedyPull.Application.DataSync.Punchup.Pages
{
    public class TicketsPage
    {
        // Global
        public readonly ILocator SeeAllButton;
        public readonly ILocator NoShowsMessage;
        
        // Bio
        public readonly ILocator BioSection;
        public readonly ILocator Name;
        public readonly ILocator Bio;
        
        // Shows
        public readonly ILocator Show;

        public TicketsPage(IPage page)
        {
            // Global
            SeeAllButton = page.GetByRole(AriaRole.Button, new PageGetByRoleOptions
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