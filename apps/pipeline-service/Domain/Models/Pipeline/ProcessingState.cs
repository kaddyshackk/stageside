using System.ComponentModel;

namespace ComedyPull.Domain.Models.Pipeline
{
    public enum ProcessingState
    {
        [Description("Created")]
        Pending,
        
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