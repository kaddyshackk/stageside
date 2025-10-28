using ComedyPull.Domain.Models;

namespace ComedyPull.Domain.Interfaces
{
    public interface ITransformer
    {
        public IEnumerable<SilverRecord> Transform(BronzeRecord record);
    }
}