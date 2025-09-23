using System.ComponentModel;

namespace ComedyPull.Domain.Models.Processing
{
    /// <summary>
    /// Represents the stage of a <see cref="SourceRecord"/> in the data pipeline.
    /// </summary>
    public enum ProcessingStage
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
        Completed
    }
}