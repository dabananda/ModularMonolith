using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using ModularMonolith.Modules.Identity.Domain.Entities;
using ModularMonolith.Modules.Identity.Infrastructure.Security;
using ModularMonolith.Shared.Common;
using ModularMonolith.Shared.Configurations;
using ModularMonolith.Shared.Services;
using Xunit;

namespace ModularMonolith.Tests
{
    public class PerformanceOptimizationsTests
    {
        private class TestItem
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        private class TestPaginationDbContext(DbContextOptions<TestPaginationDbContext> options) : DbContext(options)
        {
            public DbSet<TestItem> Items => Set<TestItem>();
        }

        [Fact]
        public async Task ToPagedResultAsync_WhenQueryIsEmpty_ShouldShortCircuitWithZeroCount()
        {
            var options = new DbContextOptionsBuilder<TestPaginationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new TestPaginationDbContext(options);

            var result = await context.Items.ToPagedResultAsync(pageNumber: 1, pageSize: 10);

            result.TotalCount.Should().Be(0);
            result.Items.Should().BeEmpty();
            result.TotalPages.Should().Be(0);
            result.HasNextPage.Should().BeFalse();
        }

        [Fact]
        public async Task ToPagedResultAsync_WhenPageExceedsTotalItems_ShouldShortCircuit()
        {
            var options = new DbContextOptionsBuilder<TestPaginationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new TestPaginationDbContext(options);
            context.Items.AddRange(
                new TestItem { Id = 1, Name = "A" },
                new TestItem { Id = 2, Name = "B" });
            await context.SaveChangesAsync();

            var result = await context.Items.ToPagedResultAsync(pageNumber: 5, pageSize: 10);

            result.TotalCount.Should().Be(2);
            result.Items.Should().BeEmpty();
            result.PageNumber.Should().Be(5);
            result.HasPreviousPage.Should().BeTrue();
            result.HasNextPage.Should().BeFalse();
        }

        [Fact]
        public async Task ToPagedResultAsync_WithValidItems_ShouldPageCorrectly()
        {
            var options = new DbContextOptionsBuilder<TestPaginationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new TestPaginationDbContext(options);
            context.Items.AddRange(
                new TestItem { Id = 1, Name = "A" },
                new TestItem { Id = 2, Name = "B" },
                new TestItem { Id = 3, Name = "C" });
            await context.SaveChangesAsync();

            var result = await context.Items.ToPagedResultAsync(pageNumber: 1, pageSize: 2);

            result.TotalCount.Should().Be(3);
            result.Items.Should().HaveCount(2);
            result.Items[0].Name.Should().Be("A");
            result.Items[1].Name.Should().Be("B");
            result.TotalPages.Should().Be(2);
            result.HasNextPage.Should().BeTrue();
        }

        [Fact]
        public void SecureTokenGenerator_ShouldGenerateUrlSafeNonEmptyUniqueTokens()
        {
            var generator = new SecureTokenGenerator();

            var token1 = generator.GenerateToken();
            var token2 = generator.GenerateToken();

            token1.Should().NotBeNullOrWhiteSpace();
            token2.Should().NotBeNullOrWhiteSpace();
            token1.Should().NotBe(token2);
            token1.Should().NotContain("+");
            token1.Should().NotContain("/");
            token1.Should().NotContain("=");
        }

        [Fact]
        public void JwtTokenGenerator_ShouldGenerateAccessTokenAndRefreshToken()
        {
            var settings = new Settings
            {
                Jwt = new JwtSettings
                {
                    Key = "A_Very_Long_And_Secure_Key_For_Testing_Purposes_Only_12345!",
                    Issuer = "TestIssuer",
                    Audience = "TestAudience",
                    AccessTokenExpiryMinutes = 15,
                    RefreshTokenExpiryDays = 7
                }
            };

            var generator = new JwtTokenGenerator(settings);
            var user = ApplicationUser.Create("user@test.com", "hash");
            var roles = new[] { "Admin", "User" };

            var (token, expiresAt) = generator.GenerateAccessToken(user, roles);
            token.Should().NotBeNullOrWhiteSpace();
            expiresAt.Should().BeAfter(DateTime.UtcNow);

            var (refreshToken, rtExpiresAt) = generator.GenerateRefreshToken();
            refreshToken.Should().NotBeNullOrWhiteSpace();
            rtExpiresAt.Should().BeAfter(DateTime.UtcNow);
        }

        [Fact]
        public async Task DistributedCacheService_ShouldSerializeAndDeserializeBinaryData()
        {
            var memoryCacheOptions = Options.Create(new MemoryDistributedCacheOptions());
            IDistributedCache memoryCache = new MemoryDistributedCache(memoryCacheOptions);
            var cacheService = new DistributedCacheService(memoryCache);

            var key = "test_key";
            var data = new TestItem { Id = 42, Name = "CachedItem" };

            await cacheService.SetAsync(key, data, TimeSpan.FromMinutes(5));

            var exists = await cacheService.ExistsAsync(key);
            exists.Should().BeTrue();

            var retrieved = await cacheService.GetAsync<TestItem>(key);
            retrieved.Should().NotBeNull();
            retrieved!.Id.Should().Be(42);
            retrieved.Name.Should().Be("CachedItem");

            await cacheService.RemoveAsync(key);
            var existsAfterRemove = await cacheService.ExistsAsync(key);
            existsAfterRemove.Should().BeFalse();
        }
    }
}
