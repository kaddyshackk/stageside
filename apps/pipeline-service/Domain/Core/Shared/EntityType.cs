using System.ComponentModel;

namespace ComedyPull.Domain.Models
{
    /// <summary>
    /// Represents an entity that can be processed and displayed.
    /// </summary>
    public enum EntityType
    {
        [Description("Act")]
        Act,
        
        [Description("Event")]
        Event,
        
        [Description("Venue")]
        Venue
    }
}