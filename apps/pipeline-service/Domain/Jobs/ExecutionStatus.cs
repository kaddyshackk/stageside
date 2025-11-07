namespace ComedyPull.Domain.Jobs
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