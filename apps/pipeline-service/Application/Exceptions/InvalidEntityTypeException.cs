using ComedyPull.Domain.Models;

namespace ComedyPull.Application.Exceptions
{
    public class InvalidEntityTypeException : Exception
    {
        public InvalidEntityTypeException(string batchId, EntityType entityType)
            : base($"Encountered invalid entity type {entityType} while processing batch {batchId}")
        {
        }
    }
}