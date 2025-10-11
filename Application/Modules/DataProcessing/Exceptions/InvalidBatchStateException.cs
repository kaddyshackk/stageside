using ComedyPull.Domain.Modules.DataProcessing;

namespace ComedyPull.Application.Modules.DataProcessing.Exceptions
{
    public class InvalidBatchStateException : Exception
    {
        public InvalidBatchStateException(string batchId, ProcessingState expectedState, ProcessingState actualState)
            : base($"Invalid batch state encountered while processing batch {batchId}. Expected: {expectedState}. Actual: {actualState}")
        {
        }
    }
}