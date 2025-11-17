namespace StageSide.Scheduler.Domain.Exceptions
{
    public class NullScheduleException(string message) : Exception(message);
}