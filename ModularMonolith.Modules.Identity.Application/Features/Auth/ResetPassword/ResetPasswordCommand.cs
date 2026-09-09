using ModularMonolith.Shared.Common;
using ModularMonolith.Shared.Messaging;

namespace ModularMonolith.Modules.Identity.Application.Features.Auth.ResetPassword
{
    public record ResetPasswordCommand(string Token, string NewPassword) : IRequest<Result>;
}