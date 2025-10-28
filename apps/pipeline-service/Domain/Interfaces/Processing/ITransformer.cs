using ComedyPull.Domain.Models.Pipeline;

namespace ComedyPull.Domain.Interfaces.Processing
{
    public interface ITransformer
    {
        public ICollection<ProcessedEntity> Transform(string data);
    }
}