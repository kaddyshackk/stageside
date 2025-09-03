namespace ComedyPull.Domain.Models
{
    public record Event
    {
        public string? Id { get; set; }

        public required string Title { get; set; }

        public required DateTimeOffset StartDateTime { get; set; }

        public required DateTimeOffset EndDateTime { get; set; }
    }
}
