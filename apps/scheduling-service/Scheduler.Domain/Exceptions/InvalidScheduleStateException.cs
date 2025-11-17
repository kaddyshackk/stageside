namespace StageSide.Scheduler.Domain.Exceptions
{
    public class InvalidScheduleStateException(string message) : Exception(message);
}