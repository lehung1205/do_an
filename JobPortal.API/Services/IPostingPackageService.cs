using JobPortal.API.DTOs;

namespace JobPortal.API.Services;

public interface IPostingPackageService
{
    Task<IReadOnlyList<PostingPackageDto>> GetAllPostingPackagesAsync(CancellationToken cancellationToken = default);
    Task<PostingPackageDto> GetPostingPackageByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<PostingPackageDto> CreatePostingPackageAsync(PostingPackageDto dto, CancellationToken cancellationToken = default);
    Task UpdatePostingPackageAsync(long id, PostingPackageDto dto, CancellationToken cancellationToken = default);
    Task DeletePostingPackageAsync(long id, CancellationToken cancellationToken = default);
}
