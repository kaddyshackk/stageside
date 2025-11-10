using System.ComponentModel;

namespace StageSide.Pipeline.Domain.Queue.Models
{
    public enum QueueKey
    {
        [Description("Collection")]
        Collection,
        
        [Description("DynamicCollection")]
        DynamicCollection,
        
        [Description("Transformation")]
        Transformation,
        
        [Description("Processing")]
        Processing
    }
}