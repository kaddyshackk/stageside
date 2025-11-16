namespace StageSide.Pipeline.Domain.Queue
{
    public enum QueueHealthStatus
    {
        Healthy,
        Warning,
        Critical,
        Overloaded
    }
}