using System.ComponentModel;

namespace StageSide.Pipeline.Domain.Queue
{
    public enum QueueKey
    {
        [Description("DynamicCollection")]
        DynamicCollection,
        
        [Description("Transformation")]
        Transformation,
        
        [Description("Processing")]
        Processing
    }
}