using System.ComponentModel;

namespace ComedyPull.Domain.Queue
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