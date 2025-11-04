using ComedyPull.Domain.Pipeline;

namespace ComedyPull.Domain.Core.Shared
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