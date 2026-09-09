using ModularMonolith.Modules.Identity.Application.Features.Auth.ForgotPassword;
using ModularMonolith.Modules.Identity.Application.Features.Auth.Login;
using ModularMonolith.Modules.Identity.Application.Features.Auth.Logout;
using ModularMonolith.Modules.Identity.Application.Features.Auth.RefreshToken;
using ModularMonolith.Modules.Identity.Application.Features.Auth.Register;
using ModularMonolith.Modules.Identity.Application.Features.Auth.ResetPassword;
using ModularMonolith.Modules.Identity.Application.Features.Auth.VerifyEmail;
using ModularMonolith.Shared.Controllers;
using ModularMonolith.Shared.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ModularMonolith.Modules.Identity.Presentation.Controllers
{
    [EnableRateLimiting("auth")]
    public class AuthController(ISender sender) : BaseController
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken cancellationToken)
        {
            return HandleResult(await sender.Send(command, cancellationToken));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
        {
            return HandleResult(await sender.Send(command, cancellationToken));
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command, CancellationToken cancellationToken)
        {
            return HandleResult(await sender.Send(command, cancellationToken));
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutCommand command, CancellationToken cancellationToken)
        {
            return HandleResult(await sender.Send(command, cancellationToken));
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] VerifyEmailCommand command, CancellationToken cancellationToken)
        {
            return HandleResult(await sender.Send(command, cancellationToken));
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command, CancellationToken cancellationToken)
        {
            return HandleResult(await sender.Send(command, cancellationToken));
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command, CancellationToken cancellationToken)
        {
            return HandleResult(await sender.Send(command, cancellationToken));
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value });
            return Ok(claims);
        }
    }
}