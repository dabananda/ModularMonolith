using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModularMonolith.Shared.Common;
using IResult = ModularMonolith.Shared.Interfaces.IResult;

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

        protected IActionResult MapError(IResult result) =>
            result.ErrorType switch
            {
                ErrorType.NotFound => NotFound(result),
                ErrorType.Unauthorized => Unauthorized(result),
                ErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, result),
                ErrorType.Conflict => Conflict(result),
                ErrorType.Validation => BadRequest(result),
                ErrorType.Failure => StatusCode(StatusCodes.Status500InternalServerError, result),
                _ => BadRequest(result)
            };
    }
}
