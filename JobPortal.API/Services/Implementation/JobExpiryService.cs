using JobPortal.API.Repositories.Interface;
using JobPortal.API.Services.Interface;

namespace JobPortal.API.Services.Implementation;

public class JobExpiryService : IJobExpiryService
{
    private readonly IJobRepository _jobRepository;

    public JobExpiryService(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public Task<int> CloseExpiredJobsAsync(CancellationToken cancellationToken = default) =>
        _jobRepository.CloseExpiredRecruitingJobsAsync(cancellationToken);
}
