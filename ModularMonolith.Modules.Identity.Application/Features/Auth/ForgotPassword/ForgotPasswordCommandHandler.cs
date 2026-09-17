using ModularMonolith.Modules.Identity.Application.Interfaces;
using ModularMonolith.Shared.Common;
using ModularMonolith.Shared.Interfaces;
using ModularMonolith.Shared.Messaging;

namespace ModularMonolith.Modules.Identity.Application.Features.Auth.ForgotPassword
{
    public class ForgotPasswordCommandHandler(
        IAuthRepository authRepository,
        IEmailService emailService,
        ICurrentUserService currentUserService,
        ISecureTokenGenerator secureTokenGenerator) : IRequestHandler<ForgotPasswordCommand, Result>
    {
        public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var ip = currentUserService.IpAddress;
            var user = await authRepository.GetByEmailAsync(request.Email, cancellationToken);

            if (user is null || !user.IsActive)
                return Result.Success();

            var resetToken = secureTokenGenerator.GenerateToken();
            var resetTokenExpiresAt = DateTime.UtcNow.AddMinutes(30);
            var passwordResetToken = user.IssuePasswordResetToken(resetToken, resetTokenExpiresAt, ip);

            await authRepository.AddPasswordResetTokenAsync(passwordResetToken, cancellationToken);
            await authRepository.SaveChangesAsync(cancellationToken);

            await emailService.SendPasswordResetLinkAsync(user.Username, user.Email, resetToken, cancellationToken);

            return Result.Success();
        }
    }
}