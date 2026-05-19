using JobPortal.API.Data;
using JobPortal.API.DTOs;
using JobPortal.API.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.API.Services.Implementation;

public class StatsService : IStatsService
{
    private readonly AppDbContext _context;

    public StatsService(AppDbContext context) => _context = context;

    public async Task<HomeStatsDto> GetHomeStatsAsync(CancellationToken cancellationToken = default)
    {
        var employerCount = await _context.Employers.CountAsync(cancellationToken);
        var jobSeekerCount = await _context.JobSeekers.CountAsync(cancellationToken);

        return new HomeStatsDto
        {
            EmployerCount = employerCount,
            JobSeekerCount = jobSeekerCount
        };
    }
}
