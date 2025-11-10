using ComedyPull.Domain.Pipeline.Models;

namespace ComedyPull.Domain.Pipeline.Interfaces
{
    public interface ITransformer
    {
        public ICollection<ProcessedEntity> Transform(object data);
    }
}