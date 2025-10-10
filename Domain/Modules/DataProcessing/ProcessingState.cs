using System.ComponentModel;

namespace ComedyPull.Domain.Modules.DataProcessing
{
    /// <summary>
    /// Represents the state of a <see cref="BronzeRecord"/> in the data pipeline.
    /// </summary>
    public enum ProcessingState
    {
        [Description("Ingested")]
        Ingested,
        
        [Description("Transformed")]
        Transformed,
        
        [Description("DeDuped")]
        DeDuped,
        
        [Description("Enriched")]
        Enriched,
        
        [Description("Linked")]
        Linked,
        
        [Description("Completed")]
        Completed,
        
        [Description("Failed")]
        Failed
    }
}