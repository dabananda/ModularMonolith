namespace ModularMonolith.Shared.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailVerificationLinkAsync(string name, string email, string token, CancellationToken cancellationToken = default);
        Task SendPasswordResetLinkAsync(string name, string email, string token, CancellationToken cancellationToken = default);
    }
}
