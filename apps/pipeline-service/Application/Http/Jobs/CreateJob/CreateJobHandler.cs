using ComedyPull.Domain.Operations;
using ComedyPull.Domain.Scheduling;

namespace ComedyPull.Application.Http.Jobs.CreateJob
{
    public class CreateJobHandler(SchedulingService jobService) : IHandler<CreateJobCommand, CreateJobResponse>
    {
        public async Task<CreateJobResponse> HandleAsync(CreateJobCommand request, CancellationToken stoppingToken)
        {
            var job = await jobService.CreateJobAsync(request, stoppingToken);
            // TODO: Handle error state where job failed to create
            return new CreateJobResponseMapper().MapToResponse(job);
        }
    }
}