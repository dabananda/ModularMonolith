using FluentAssertions;
using ModularMonolith.Modules.Identity.Domain.Entities;
using Xunit;

namespace ModularMonolith.Tests
{
    public class ApplicationUserDomainTests
    {
        [Fact]
        public void Create_ShouldNormalizeEmailAndSetDefaults()
        {
            var user = ApplicationUser.Create("  Test.User@EXAMPLE.COM  ", "hashed_pwd_123");

            user.Email.Should().Be("test.user@example.com");
            user.Username.Should().Be("test.user@example.com");
            user.PasswordHash.Should().Be("hashed_pwd_123");
            user.EmailConfirmed.Should().BeFalse();
            user.IsActive.Should().BeTrue();
        }

        [Theory]
        [InlineData("", "pwd")]
        [InlineData("   ", "pwd")]
        [InlineData("email@test.com", "")]
        [InlineData("email@test.com", "   ")]
        public void Create_WithInvalidArguments_ShouldThrowArgumentException(string email, string password)
        {
            var act = () => ApplicationUser.Create(email, password);

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ConfirmEmail_ShouldSetEmailConfirmedTrue()
        {
            var user = ApplicationUser.Create("user@test.com", "hash");

            user.ConfirmEmail();

            user.EmailConfirmed.Should().BeTrue();
        }

        [Fact]
        public void RecordLogin_WhenActive_ShouldUpdateLastLoginAt()
        {
            var user = ApplicationUser.Create("user@test.com", "hash");

            user.RecordLogin();

            user.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void RecordLogin_WhenDeactivated_ShouldThrowInvalidOperationException()
        {
            var user = ApplicationUser.Create("user@test.com", "hash");
            user.Deactivate();

            var act = () => user.RecordLogin();

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Cannot log in. Account is deactivated.");
        }

        [Fact]
        public void Roles_AssignAndRemove_ShouldWorkAsExpected()
        {
            var user = ApplicationUser.Create("user@test.com", "hash");
            var role = Role.Create("Manager", "Manager role");

            user.AssignRole(role);
            user.UserRoles.Should().HaveCount(1);
            user.UserRoles.First().RoleId.Should().Be(role.Id);

            // duplicate assignment ignored
            user.AssignRole(role);
            user.UserRoles.Should().HaveCount(1);

            // remove role
            user.RemoveRole(role.Id);
            user.UserRoles.Should().BeEmpty();
        }

        [Fact]
        public void RefreshTokens_IssueAndRevoke_ShouldTrackStatusCorrectly()
        {
            var user = ApplicationUser.Create("user@test.com", "hash");
            var expiry = DateTime.UtcNow.AddDays(7);

            var token = user.IssueRefreshToken("token_123", expiry, "127.0.0.1");

            token.IsActive.Should().BeTrue();
            token.IsRevoked.Should().BeFalse();
            token.IsExpired.Should().BeFalse();

            user.RevokeRefreshToken("token_123", "127.0.0.1", "new_token_456");

            token.IsActive.Should().BeFalse();
            token.IsRevoked.Should().BeTrue();
            token.ReplacedByToken.Should().Be("new_token_456");
        }

        [Fact]
        public void ResetPassword_ShouldChangeHashAndRevokeAllActiveRefreshTokens()
        {
            var user = ApplicationUser.Create("user@test.com", "old_hash");
            var resetToken = user.IssuePasswordResetToken("reset_token_1", DateTime.UtcNow.AddHours(1), "127.0.0.1");
            var rt1 = user.IssueRefreshToken("rt_1", DateTime.UtcNow.AddDays(1), "127.0.0.1");
            var rt2 = user.IssueRefreshToken("rt_2", DateTime.UtcNow.AddDays(1), "127.0.0.1");

            user.ResetPassword("reset_token_1", "new_hash");

            user.PasswordHash.Should().Be("new_hash");
            resetToken.IsConsumed.Should().BeTrue();
            rt1.IsRevoked.Should().BeTrue();
            rt2.IsRevoked.Should().BeTrue();
        }
    }
}
