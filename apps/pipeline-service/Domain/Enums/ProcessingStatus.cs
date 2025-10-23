using System.ComponentModel;
using ComedyPull.Domain.Modules.DataProcessing;

namespace ComedyPull.Domain.Enums
{
    /// <summary>
    /// Represents the status of a <see cref="BronzeRecord"/> in the data pipeline.
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