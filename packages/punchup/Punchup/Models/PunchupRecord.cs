namespace StageSide.Punchup.Models
{
    public record PunchupRecord
    {
        public required string Name { get; init; }

        public required string Bio { get; init; }

        public required List<PunchupEvent> Events { get; init; }
    }
}