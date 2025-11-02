namespace ComedyPull.Domain.Models
{
    public record BatchProcessResult<TOrigin, TResult>
    {
        public required IEnumerable<TResult> Created { get; init; }
        public required IEnumerable<TResult> Updated { get; init; }
        public required IEnumerable<TOrigin> Failed { get; init; }
        public required int ProcessedCount { get; init; }
    }
}