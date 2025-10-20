using ComedyPull.Domain.Enums;

namespace ComedyPull.Application.Modules.DataProcessing.Exceptions
{
    public class InvalidBatchStateException(string batchId, ProcessingState expectedState, ProcessingState actualState)
        : Exception(
            $"Invalid batch state encountered while processing batch {batchId}. Expected: {expectedState}. Actual: {actualState}");
}