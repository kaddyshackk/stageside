namespace Domain.Models
{
    internal class Event
    {
        public string? Id { get; set; }

        public required string Title { get; set; }

        public required DateTimeOffset StartTime { get; set; }

        public required DateTimeOffset EndTime { get; set; }
    }
}
