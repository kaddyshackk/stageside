using ComedyPull.Domain.Models.Queue;

namespace ComedyPull.Application.Pipeline
{
    public class BackPressureOptions
    {
        public bool EnableBackPressure { get; init; }
        public bool EnableAdaptiveBatching { get; init; }
        public required Dictionary<string, QueueThresholds> QueueThresholds { get; init; }
    }
}