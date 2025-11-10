using StageSide.Pipeline.Domain.Scheduling.Models;
using Riok.Mapperly.Abstractions;

namespace StageSide.Pipeline.Service.Operations.Scheduling.CreateJob
{
    [Mapper]
    public partial class CreateJobResponseMapper
    {
        [MapperIgnoreSource("Sitemaps")]
        [MapperIgnoreSource("Executions")]
        [MapperIgnoreSource("CreatedAt")]
        [MapperIgnoreSource("CreatedBy")]
        [MapperIgnoreSource("UpdatedAt")]
        [MapperIgnoreSource("UpdatedBy")]
        public partial CreateJobResponse MapToResponse(Job job);
    }
}