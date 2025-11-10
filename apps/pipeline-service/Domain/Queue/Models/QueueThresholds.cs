namespace ComedyPull.Domain.Queue.Models
{
    public class QueueThresholds
    {
        public int Normal { get; init; }
        public int Warning { get; init; }
        public int Critical { get; init; }
    }
}