using JobPortal.API.DTOs;

namespace JobPortal.API.Services.Interface;

public interface IStatsService
{
    Task<HomeStatsDto> GetHomeStatsAsync(CancellationToken cancellationToken = default);

    Task<TopRatedListsDto> GetTopRatedListsAsync(CancellationToken cancellationToken = default);
}
