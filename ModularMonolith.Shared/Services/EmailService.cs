using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;
using ModularMonolith.Shared.Configurations;
using ModularMonolith.Shared.Interfaces;
using System.Net;

namespace ModularMonolith.Shared.Services
{
    internal class EmailService(Settings settings, ILogger<EmailService> logger) : IEmailService
    {
        public async Task SendEmailVerificationLinkAsync(string name, string email, string token, CancellationToken cancellationToken = default)
        {
            var link = BuildVerificationLink(token);

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
            var link = BuildPasswordResetLink(token);

            var body = $"""
                <p>Hi {WebUtility.HtmlEncode(name)},</p>
                <p>We received a request to reset your password. Click the link below to choose a new one:</p>
                <p><a href="{link}">Reset my password</a></p>
                <p>This link will expire in 30 minutes. If you didn't request this, you can safely ignore this email.</p>
                """;

            await SendAsync(email, "Reset your password", body, cancellationToken);
        }

        private string BuildVerificationLink(string token)
        {
            var baseUrl = string.IsNullOrWhiteSpace(settings.Email.BaseUrl) ? "http://localhost:5259" : settings.Email.BaseUrl.TrimEnd('/');
            return $"{baseUrl}/api/v1/auth/verify-email?token={Uri.EscapeDataString(token)}";
        }

        private string BuildPasswordResetLink(string token)
        {
            var clientUrl = string.IsNullOrWhiteSpace(settings.Email.ClientUrl) ? "http://localhost:3000" : settings.Email.ClientUrl.TrimEnd('/');
            return $"{clientUrl}/reset-password?token={Uri.EscapeDataString(token)}";
        }

        private async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(settings.Email.Host))
            {
                logger.LogWarning("Email sending skipped: SMTP Host is not configured. Target: {ToEmail}, Subject: {Subject}", toEmail, subject);
                return;
            }

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
