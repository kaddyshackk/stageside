namespace StageSide.Pipeline.Domain.Exceptions
{
    public class InvalidExecutionStateException(string message) : Exception(message);
}