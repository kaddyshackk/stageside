namespace ComedyPull.Domain.Pipeline
{
    public record BatchProcessResult<TOrigin, TResult>
    {
        public IEnumerable<TResult> Created { get; init; } = [];
        public IEnumerable<TResult> Updated { get; init; } = [];
        public IEnumerable<TOrigin> Failed { get; init; } = [];
        public int ProcessedCount { get; init; } = 0;
    }
}