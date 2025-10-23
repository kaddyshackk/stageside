using ComedyPull.Domain.Enums;

namespace ComedyPull.Application.Modules.DataProcessing.Exceptions
{
    public class InvalidEntityTypeException : Exception
    {
        public InvalidEntityTypeException(string batchId, EntityType entityType)
            : base($"Encountered invalid entity type {entityType} while processing batch {batchId}")
        {
        }
    }
}