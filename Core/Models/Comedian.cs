namespace Domain.Models
{
    internal class Comedian
    {
        public string? Id { get; set; }

        public required string slug { get; set; }

        public required string Name { get; set; }
    }
}
