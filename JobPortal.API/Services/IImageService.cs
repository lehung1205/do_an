using JobPortal.API.DTOs;

namespace JobPortal.API.Services;

public interface IImageService
{
    Task<IReadOnlyList<ImageDto>> GetAllImagesAsync(CancellationToken cancellationToken = default);
    Task<ImageDto> GetImageByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<ImageDto> CreateImageAsync(ImageDto dto, CancellationToken cancellationToken = default);
    Task UpdateImageAsync(long id, ImageDto dto, CancellationToken cancellationToken = default);
    Task DeleteImageAsync(long id, CancellationToken cancellationToken = default);
}
