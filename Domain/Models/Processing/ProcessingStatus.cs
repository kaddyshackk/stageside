using System.ComponentModel;

namespace ComedyPull.Domain.Models.Processing
{
    /// <summary>
    /// Represents the status of a <see cref="SourceRecord"/> in the data pipeline.
    /// </summary>
    public enum ProcessingStatus
    {
        [Description("Processing")]
        Processing,
        
        [Description("Completed")]
        Completed,
        
        [Description("Failed")]
        Failed,
        
        [Description("Skipped")]
        Skipped
    }
}