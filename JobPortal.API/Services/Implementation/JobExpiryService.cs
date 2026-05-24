using JobPortal.API.Helpers;
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

    public async Task<int> CloseExpiredJobsAsync(CancellationToken cancellationToken = default)
    {
        await AutoApproveStalePendingJobsAsync(cancellationToken);
        return await _jobRepository.CloseExpiredRecruitingJobsAsync(cancellationToken);
    }

    public Task<int> AutoApproveStalePendingJobsAsync(CancellationToken cancellationToken = default) =>
        _jobRepository.AutoApproveStalePendingJobsAsync(JobPostingCatalog.PendingAutoApproveDelay, cancellationToken);
}
