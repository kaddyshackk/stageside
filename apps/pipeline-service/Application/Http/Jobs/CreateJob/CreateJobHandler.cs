using ComedyPull.Domain.Jobs.Operations.CreateJob;
using ComedyPull.Domain.Jobs.Services;

namespace ComedyPull.Application.Http.Jobs.CreateJob
{
    public class CreateJobHandler(JobService jobService) : IHandler<CreateJobCommand, CreateJobResponse>
    {
        public async Task<CreateJobResponse> HandleAsync(CreateJobCommand request, CancellationToken stoppingToken)
        {
            var job = await jobService.CreateJobAsync(request, stoppingToken);
            return new CreateJobResponseMapper().MapToResponse(job);
        }
    }
}