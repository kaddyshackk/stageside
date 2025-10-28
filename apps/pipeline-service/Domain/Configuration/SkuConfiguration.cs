using ComedyPull.Domain.Models;
using ComedyPull.Domain.Models.Pipeline;

namespace ComedyPull.Domain.Configuration
{
    public static class SkuConfiguration
    {
        private static readonly Dictionary<Sku, CollectionType> CollectionTypes = new()
        {
            { Sku.PunchupTicketsPage, CollectionType.Dynamic }
        };

        public static CollectionType GetCollectionType(Sku sku)
        {
            return CollectionTypes.TryGetValue(sku, out var collectionType)
                ? collectionType
                : throw new NotSupportedException($"No collection type configured for SKU: {sku}");
        }
    }
}