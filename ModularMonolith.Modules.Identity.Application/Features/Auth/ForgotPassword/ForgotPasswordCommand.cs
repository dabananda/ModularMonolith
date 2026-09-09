using ModularMonolith.Shared.Common;
using ModularMonolith.Shared.Messaging;

namespace ModularMonolith.Modules.Identity.Application.Features.Auth.ForgotPassword
{
    public record ForgotPasswordCommand(string Email) : IRequest<Result>;
}