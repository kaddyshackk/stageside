namespace StageSide.Scheduler.Domain.Exceptions
{
    public class InvalidJobStateException(string message) : Exception(message);
}