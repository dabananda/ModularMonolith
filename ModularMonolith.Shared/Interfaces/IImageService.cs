using ModularMonolith.Shared.Common;
using Microsoft.AspNetCore.Http;

namespace ModularMonolith.Shared.Interfaces
{
    public interface IImageService
    {
        Task<ImageUploadResult> UploadAsync(IFormFile file);
        Task<Result> UpdateEntityIdAsync(Guid imageId, Guid entityId);
        Task<Result> DeleteAsync(Guid id);
    }
}