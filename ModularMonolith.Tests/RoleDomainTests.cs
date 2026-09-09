using FluentAssertions;
using ModularMonolith.Modules.Identity.Domain.Entities;
using Xunit;

namespace ModularMonolith.Tests
{
    public class RoleDomainTests
    {
        [Fact]
        public void Create_WithValidName_ShouldCreateRole()
        {
            var role = Role.Create("Editor", "Editor role");

            role.Name.Should().Be("Editor");
            role.Description.Should().Be("Editor role");
            role.IsDeleted.Should().BeFalse();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_WithEmptyName_ShouldThrow(string name)
        {
            var act = () => Role.Create(name);

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void UpdateDetails_ShouldUpdateNameAndDescription()
        {
            var role = Role.Create("Author", "Initial description");

            role.UpdateDetails("Lead Author", "Updated description");

            role.Name.Should().Be("Lead Author");
            role.Description.Should().Be("Updated description");
        }

        [Fact]
        public void MarkDelete_ShouldSetIsDeletedTrue()
        {
            var role = Role.Create("Temporary");

            role.MarkDelete();

            role.IsDeleted.Should().BeTrue();
        }
    }
}
