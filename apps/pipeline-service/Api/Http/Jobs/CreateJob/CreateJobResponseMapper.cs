using ComedyPull.Domain.Scheduling.Models;
using Riok.Mapperly.Abstractions;

namespace ComedyPull.Api.Http.Jobs.CreateJob
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