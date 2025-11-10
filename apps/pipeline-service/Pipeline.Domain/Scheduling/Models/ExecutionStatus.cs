namespace StageSide.Pipeline.Domain.Scheduling.Models
{
    public enum ExecutionStatus
    {
        Created,
        Executed,
        Completed,
        Failed,
        Cancelled
    }
}