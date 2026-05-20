namespace JobPortal.API.Services.Interface;

public interface IJobExpiryService
{
    /// <summary>
    /// Sets posting_status to closed for recruiting jobs past expiry_date. Returns rows updated.</summary>
    Task<int> CloseExpiredJobsAsync(CancellationToken cancellationToken = default);
}
