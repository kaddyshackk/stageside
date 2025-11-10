namespace StageSide.Pipeline.Domain.Queue.Models
{
    public enum QueueHealthStatus
    {
        Healthy,
        Warning,
        Critical,
        Overloaded
    }
}