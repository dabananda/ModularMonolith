using ModularMonolith.Shared.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ModularMonolith.Shared.Controllers
{
    public class ImageController(IImageService imageService) : BaseController
    {
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile image, CancellationToken cancellationToken)
        {
            var result = await imageService.UploadAsync(image, cancellationToken);
            return HandleResult(result);
        }

        [HttpDelete("delete/{id:guid}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await imageService.DeleteAsync(id, cancellationToken);
            return HandleResult(result);
        }
    }
}
