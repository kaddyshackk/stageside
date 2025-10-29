namespace ComedyPull.Domain.Models.Pipeline
{
    public enum JobExecutionStatus
    {
        Scheduled,
        Completed,
        Failed,
        Cancelled
    }
}