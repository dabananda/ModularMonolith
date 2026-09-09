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
        public async Task<IActionResult> Register(RegisterCommand command)
        {
            return HandleResult(await sender.Send(command));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginCommand command)
        {
            return HandleResult(await sender.Send(command));
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenCommand command)
        {
            return HandleResult(await sender.Send(command));
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(LogoutCommand command)
        {
            return HandleResult(await sender.Send(command));
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] VerifyEmailCommand command)
        {
            return HandleResult(await sender.Send(command));
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordCommand command)
        {
            return HandleResult(await sender.Send(command));
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordCommand command)
        {
            return HandleResult(await sender.Send(command));
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