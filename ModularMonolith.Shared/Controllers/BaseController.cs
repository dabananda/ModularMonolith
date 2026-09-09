using ModularMonolith.Shared.Common;
using ModularMonolith.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ModularMonolith.Shared.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public abstract class BaseController : ControllerBase
    {
        protected IActionResult HandleResult<T>(Result<T> result)
        {
            if (result.IsSuccess)
                return Ok(result);

            return MapError(result);
        }

        protected IActionResult HandleResult(Result result)
        {
            if (result.IsSuccess)
                return Ok(result);

            return MapError(result);
        }

        protected IActionResult HandleCreatedResult<T>(Result<T> result, string actionName, object? routeValues = null)
        {
            if (result.IsSuccess)
                return CreatedAtAction(actionName, routeValues, result);

            return MapError(result);
        }

        private IActionResult MapError(IResult result) =>
            result.ErrorType switch
            {
                ErrorType.NotFound => NotFound(result),
                ErrorType.Unauthorized => Unauthorized(result),
                ErrorType.Forbidden => Forbid(),
                ErrorType.Conflict => Conflict(result),
                ErrorType.Validation => BadRequest(result),
                _ => BadRequest(result)
            };
    }
}
