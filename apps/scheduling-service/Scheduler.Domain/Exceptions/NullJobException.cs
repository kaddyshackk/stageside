namespace StageSide.Scheduler.Domain.Exceptions
{
    public class NullJobException(string message) : Exception(message);
}