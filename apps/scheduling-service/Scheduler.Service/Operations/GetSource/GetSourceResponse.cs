namespace StageSide.Scheduler.Service.Operations.GetSource
{
    public class GetSourceResponse
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string Website { get; set; }
        public ICollection<GetSourceSkuResponse> Skus { get; set; }
    }
}