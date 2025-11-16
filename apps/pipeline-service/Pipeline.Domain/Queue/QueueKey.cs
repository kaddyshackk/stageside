using System.ComponentModel;

namespace StageSide.Pipeline.Domain.Queue
{
    public enum QueueKey
    {
        [Description("Collection")]
        Collection,
        
        [Description("Transformation")]
        Transformation,
        
        [Description("Processing")]
        Processing
    }
}