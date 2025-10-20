using System.ComponentModel;

namespace ComedyPull.Domain.Modules.DataProcessing
{
    /// <summary>
    /// Represents the state of a <see cref="Batch"/> in the data pipeline.
    /// </summary>
    public enum ProcessingState
    {
        [Description("Created")]
        Created,
        
        [Description("Ingested")]
        Ingested,
        
        [Description("Transformed")]
        Transformed,
        
        [Description("Completed")]
        Completed,
        
        [Description("Failed")]
        Failed
    }
}