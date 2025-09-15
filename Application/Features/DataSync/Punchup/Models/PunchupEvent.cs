namespace ComedyPull.Application.Features.DataSync.Punchup.Models
{
    public record PunchupEvent
    {
        public required DateTimeOffset StartDateTime { get; init; }

        public required string Location { get; init; }

        public required string Venue { get; init; }

        public string? TicketLink { get; init; }
    }
}