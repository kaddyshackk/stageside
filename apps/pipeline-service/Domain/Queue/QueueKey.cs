using System.ComponentModel;

namespace ComedyPull.Domain.Models.Queue
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