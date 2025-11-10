using ComedyPull.Domain.Extensions;
using ComedyPull.Domain.Pipeline.Models;

namespace ComedyPull.Domain.Queue.Models
{
    public static class Queues
    {
        public static readonly QueueConfig<PipelineContext> Collection = new(QueueKey.Collection.GetEnumDescription());
        public static readonly QueueConfig<PipelineContext> DynamicCollection = new(QueueKey.DynamicCollection.GetEnumDescription());
        public static readonly QueueConfig<PipelineContext> Transformation = new(QueueKey.Transformation.GetEnumDescription());
        public static readonly QueueConfig<PipelineContext> Processing = new(QueueKey.Processing.GetEnumDescription());
    }
}