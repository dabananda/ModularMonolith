using ModularMonolith.Shared.Common;
using ModularMonolith.Shared.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ModularMonolith.Shared.Controllers
{
    public class ImageController(IImageService imageService) : BaseController
    {
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile image)
        {
            var result = await imageService.UploadAsync(image);
            return HandleResult(Result<ImageUploadResult>.Success(result));
        }

        [HttpDelete("delete/{id:guid}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] Guid id)
        {
            var result = await imageService.DeleteAsync(id);
            return HandleResult(result);
        }
    }
}
