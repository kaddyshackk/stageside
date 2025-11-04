namespace ComedyPull.Domain.Pipeline.Interfaces
{
    public interface ITransformer
    {
        public ICollection<ProcessedEntity> Transform(string data);
    }
}