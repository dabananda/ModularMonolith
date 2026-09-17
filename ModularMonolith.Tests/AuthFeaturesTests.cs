using FluentAssertions;
using ModularMonolith.Modules.Identity.Application.Common;
using ModularMonolith.Modules.Identity.Application.Features.Auth.Login;
using ModularMonolith.Modules.Identity.Application.Features.Auth.Register;
using ModularMonolith.Modules.Identity.Application.Features.Role.Assign;
using ModularMonolith.Modules.Identity.Application.Features.Role.Remove;
using ModularMonolith.Modules.Identity.Application.Interfaces;
using ModularMonolith.Modules.Identity.Domain.Entities;
using ModularMonolith.Shared.Common;
using ModularMonolith.Shared.Interfaces;
using Moq;
using Xunit;

namespace ModularMonolith.Tests
{
    public class AuthFeaturesTests
    {
        private readonly Mock<IAuthRepository> _authRepoMock = new();
        private readonly Mock<IRoleRepository> _roleRepoMock = new();
        private readonly Mock<IPasswordHasher> _hasherMock = new();
        private readonly Mock<IJwtTokenGenerator> _jwtMock = new();
        private readonly Mock<ISecureTokenGenerator> _tokenGenMock = new();
        private readonly Mock<IEmailService> _emailServiceMock = new();
        private readonly Mock<ICurrentUserService> _currentUserMock = new();

        public AuthFeaturesTests()
        {
            _currentUserMock.Setup(x => x.IpAddress).Returns("127.0.0.1");
            _currentUserMock.Setup(x => x.UserId).Returns(Guid.NewGuid());
        }

        [Fact]
        public async Task Login_WhenCredentialsInvalid_ShouldReturnUnauthorized()
        {
            _authRepoMock.Setup(x => x.GetByEmailAsync("user@test.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync((ApplicationUser?)null);

            var handler = new LoginCommandHandler(
                _authRepoMock.Object,
                _hasherMock.Object,
                _jwtMock.Object,
                _currentUserMock.Object);

            var result = await handler.Handle(new LoginCommand("user@test.com", "wrongpwd"), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.ErrorType.Should().Be(ErrorType.Unauthorized);
            result.Message.Should().Be(AuthErrors.InvalidCredentials);
        }

        [Fact]
        public async Task Login_WhenEmailNotConfirmed_ShouldReturnForbidden()
        {
            var user = ApplicationUser.Create("user@test.com", "hash");
            _authRepoMock.Setup(x => x.GetByEmailAsync("user@test.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _hasherMock.Setup(x => x.VerifyPassword("password", "hash")).Returns(true);

            var handler = new LoginCommandHandler(
                _authRepoMock.Object,
                _hasherMock.Object,
                _jwtMock.Object,
                _currentUserMock.Object);

            var result = await handler.Handle(new LoginCommand("user@test.com", "password"), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.ErrorType.Should().Be(ErrorType.Forbidden);
            result.Message.Should().Be(AuthErrors.EmailNotConfirmed);
        }

        [Fact]
        public async Task Login_WhenValid_ShouldIssueTokensAndReturnSuccess()
        {
            var user = ApplicationUser.Create("user@test.com", "hash");
            user.ConfirmEmail();

            _authRepoMock.Setup(x => x.GetByEmailAsync("user@test.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _hasherMock.Setup(x => x.VerifyPassword("password", "hash")).Returns(true);
            _jwtMock.Setup(x => x.GenerateAccessToken(user, It.IsAny<IEnumerable<string>>()))
                .Returns(("access_token_123", DateTime.UtcNow.AddMinutes(15)));
            _jwtMock.Setup(x => x.GenerateRefreshToken())
                .Returns(("refresh_token_123", DateTime.UtcNow.AddDays(7)));

            var handler = new LoginCommandHandler(
                _authRepoMock.Object,
                _hasherMock.Object,
                _jwtMock.Object,
                _currentUserMock.Object);

            var result = await handler.Handle(new LoginCommand("user@test.com", "password"), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.AccessToken.Should().Be("access_token_123");
            result.Data!.RefreshToken.Should().Be("refresh_token_123");
            _authRepoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Register_WhenEmailAlreadyExists_ShouldReturnConflict()
        {
            _authRepoMock.Setup(x => x.EmailExistsAsync("existing@test.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var handler = new RegisterCommandHandler(
                _authRepoMock.Object,
                _roleRepoMock.Object,
                _hasherMock.Object,
                _tokenGenMock.Object,
                _emailServiceMock.Object);

            var result = await handler.Handle(new RegisterCommand("existing@test.com", "Pass123!"), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.ErrorType.Should().Be(ErrorType.Conflict);
            result.Message.Should().Be(AuthErrors.EmailAlreadyExists);
        }

        [Fact]
        public async Task AssignRole_WhenUserAndRoleExist_ShouldAssignAndReturnSuccess()
        {
            var user = ApplicationUser.Create("user@test.com", "hash");
            var role = Role.Create("Manager");

            _authRepoMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _roleRepoMock.Setup(x => x.GetByIdAsync(role.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(role);

            var handler = new AssignRoleCommandHandler(
                _authRepoMock.Object,
                _roleRepoMock.Object);

            var result = await handler.Handle(new AssignRoleCommand(user.Id, role.Id), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            user.UserRoles.Should().Contain(ur => ur.RoleId == role.Id);
            _authRepoMock.Verify(x => x.Update(user), Times.Once);
            _authRepoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RemoveRole_WhenRoleAssigned_ShouldRemoveAndReturnSuccess()
        {
            var user = ApplicationUser.Create("user@test.com", "hash");
            var role = Role.Create("Manager");
            user.AssignRole(role);

            _authRepoMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _roleRepoMock.Setup(x => x.GetByIdAsync(role.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(role);

            var handler = new RemoveRoleCommandHandler(
                _authRepoMock.Object,
                _roleRepoMock.Object);

            var result = await handler.Handle(new RemoveRoleCommand(user.Id, role.Id), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            user.UserRoles.Should().NotContain(ur => ur.RoleId == role.Id);
            _authRepoMock.Verify(x => x.Update(user), Times.Once);
            _authRepoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
