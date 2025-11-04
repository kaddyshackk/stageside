namespace ComedyPull.Application.Pipeline
{
    public class BackPressureOptions
    {
        public bool EnableBackPressure { get; init; }
        public bool EnableAdaptiveBatching { get; init; }
    }
}