using ComedyPull.Domain.Operations;
using Riok.Mapperly.Abstractions;

namespace ComedyPull.Api.Http.Jobs.CreateJob
{
    [Mapper]
    public partial class CreateJobRequestMapper
    {
        public partial CreateJobCommand MapToCommand(CreateJobRequest req);
    }
}