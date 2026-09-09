using Microsoft.AspNetCore.Http;
using ModularMonolith.Shared.Common;

namespace ModularMonolith.Shared.Interfaces
{
    public interface IImageService
    {
        Task<Result<ImageUploadResult>> UploadAsync(IFormFile file, CancellationToken cancellationToken = default);
        Task<Result> UpdateEntityIdAsync(Guid imageId, Guid entityId, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}