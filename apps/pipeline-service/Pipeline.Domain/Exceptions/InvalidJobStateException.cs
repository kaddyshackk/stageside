namespace StageSide.Pipeline.Domain.Exceptions
{
    public class InvalidJobStateException(string message) : Exception(message);
}