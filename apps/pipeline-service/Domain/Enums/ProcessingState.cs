using System.ComponentModel;
using ComedyPull.Domain.Modules.DataProcessing;

namespace ComedyPull.Domain.Enums
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