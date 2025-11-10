namespace StageSide.Pipeline.Domain.Exceptions
{
    public class InvalidJobExecutionStateException(string message) : Exception(message);
}