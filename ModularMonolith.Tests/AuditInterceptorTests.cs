using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.Entities;
using ModularMonolith.Shared.Interfaces;
using ModularMonolith.Shared.Persistence;
using Moq;
using Xunit;

namespace ModularMonolith.Tests
{
    public class AuditInterceptorTests
    {
        public class TestAuditableEntity : BaseEntity
        {
            public string Name { get; set; } = string.Empty;
        }

        public class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
        {
            public DbSet<TestAuditableEntity> Entities => Set<TestAuditableEntity>();
        }

        [Fact]
        public async Task Interceptor_ShouldSetAuditFields_OnAddAndModifyAndSoftDelete()
        {
            var expectedUserId = Guid.NewGuid();
            var currentUserServiceMock = new Mock<ICurrentUserService>();
            currentUserServiceMock.Setup(x => x.UserId).Returns(expectedUserId);

            var interceptor = new AuditSaveChangesInterceptor(currentUserServiceMock.Object);

            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .AddInterceptors(interceptor)
                .Options;

            using var context = new TestDbContext(options);

            // 1. Test Added
            var entity = new TestAuditableEntity { Name = "First" };
            context.Entities.Add(entity);
            await context.SaveChangesAsync();

            entity.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
            entity.CreatedBy.Should().Be(expectedUserId);
            entity.UpdatedAt.Should().BeNull();
            entity.IsDeleted.Should().BeFalse();

            // 2. Test Modified
            entity.Name = "Updated";
            await context.SaveChangesAsync();

            entity.UpdatedAt.Should().NotBeNull();
            entity.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
            entity.UpdatedBy.Should().Be(expectedUserId);

            // 3. Test Soft Delete via context.Remove
            context.Entities.Remove(entity);
            await context.SaveChangesAsync();

            entity.IsDeleted.Should().BeTrue();
            entity.DeletedAt.Should().NotBeNull();
            entity.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
            entity.DeletedBy.Should().Be(expectedUserId);
        }
    }
}
