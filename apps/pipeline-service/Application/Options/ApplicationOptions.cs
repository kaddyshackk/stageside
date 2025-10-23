namespace ComedyPull.Application.Options
{
    public class ApplicationOptions
    {
        public DataSyncOptions DataSync { get; init; } = new();

        public PunchupOptions Punchup { get; init; } = new();
    }
}