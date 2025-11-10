using System.ComponentModel;

namespace ComedyPull.Domain.Pipeline.Models
{
    public enum ProcessingState
    {
        [Description("Created")]
        Pending,
        
        [Description("Collected")]
        Collected,
        
        [Description("Transformed")]
        Transformed,
        
        [Description("Completed")]
        Completed,
        
        [Description("Failed")]
        Failed
    }
}