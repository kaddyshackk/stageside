using ComedyPull.Domain.Extensions;
using ComedyPull.Domain.Models.Pipeline;

namespace ComedyPull.Domain.Models.Queue
{
    public static class Queues
    {
        public static readonly QueueConfig<CollectionRequest> Collection = new(QueueKey.Collection.GetEnumDescription());
        public static readonly QueueConfig<CollectionRequest> DynamicCollection = new(QueueKey.DynamicCollection.GetEnumDescription());
        public static readonly QueueConfig<BronzeRecord> Transformation = new(QueueKey.Transformation.GetEnumDescription());
        public static readonly QueueConfig<SilverRecord> Processing = new(QueueKey.Processing.GetEnumDescription());
    }
}