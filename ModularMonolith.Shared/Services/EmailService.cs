using ModularMonolith.Shared.Configurations;
using ModularMonolith.Shared.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Net;

namespace ModularMonolith.Shared.Services
{
    internal class EmailService(Settings settings) : IEmailService
    {
        public async Task SendEmailVerificationLinkAsync(string name, string email, string token, CancellationToken cancellationToken = default)
        {
            var link = BuildLink(token);

            var body = $"""
                <p>Hi {WebUtility.HtmlEncode(name)},</p>
                <p>Thanks for registering. Please confirm your email address by clicking the link below:</p>
                <p><a href="{link}">Verify my email</a></p>
                <p>This link will expire in 24 hours. If you didn't create this account, you can safely ignore this email.</p>
                """;

            await SendAsync(email, "Verify your email address", body, cancellationToken);
        }

        public async Task SendPasswordResetLinkAsync(string name, string email, string token, CancellationToken cancellationToken = default)
        {
            var link = BuildLink(token);

            var body = $"""
                <p>Hi {WebUtility.HtmlEncode(name)},</p>
                <p>We received a request to reset your password. Click the link below to choose a new one:</p>
                <p><a href="{link}">Reset my password</a></p>
                <p>This link will expire in 30 minutes. If you didn't request this, you can safely ignore this email.</p>
                """;

            await SendAsync(email, "Reset your password", body, cancellationToken);
        }

        private static string BuildLink(string token)
        {
            return $"https://localhost:5001/api/v1/auth/confirm-email" + $"?token={token}";
        }

        private async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken)
        {
            var email = new MimeMessage();

            email.From.Add(new MailboxAddress(settings.Email.FromName, settings.Email.From));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;
            email.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(settings.Email.Host, settings.Email.Port, SecureSocketOptions.StartTls, cancellationToken);
            await smtp.AuthenticateAsync(settings.Email.Username, settings.Email.Password, cancellationToken);
            await smtp.SendAsync(email, cancellationToken);
            await smtp.DisconnectAsync(true, cancellationToken);
        }
    }
}
