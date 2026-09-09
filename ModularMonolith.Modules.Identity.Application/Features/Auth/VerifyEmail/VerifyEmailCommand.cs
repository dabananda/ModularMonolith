using ModularMonolith.Shared.Common;
using ModularMonolith.Shared.Messaging;

namespace ModularMonolith.Modules.Identity.Application.Features.Auth.VerifyEmail
{
    public record VerifyEmailCommand(string Token) : IRequest<Result>;
}