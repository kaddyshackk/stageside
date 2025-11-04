namespace ComedyPull.Domain.Exceptions
{
    public class InvalidJobExecutionStateException(string message) : Exception(message);
}