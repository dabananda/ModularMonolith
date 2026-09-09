using FluentAssertions;
using ModularMonolith.Shared.Common;
using Xunit;

namespace ModularMonolith.Tests
{
    public class CommonTests
    {
        [Fact]
        public void Result_Success_ShouldHaveIsSuccessTrue()
        {
            var result = Result.Success("Operation completed");

            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Be("Operation completed");
            result.ErrorType.Should().BeNull();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Result_Failure_ShouldHaveIsSuccessFalseAndErrorType()
        {
            var errors = new[] { "Field required", "Invalid format" };
            var result = Result.Failure(ErrorType.Validation, "Validation failed", errors);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("Validation failed");
            result.ErrorType.Should().Be(ErrorType.Validation);
            result.Errors.Should().BeEquivalentTo(errors);
        }

        [Fact]
        public void ResultT_Success_ShouldContainData()
        {
            var data = new { Id = 123, Name = "Test" };
            var result = Result<object>.Success(data, "Created");

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(data);
            result.Message.Should().Be("Created");
        }

        [Fact]
        public void Slug_Generate_ShouldConvertAccentedCharactersAndSymbols()
        {
            var slug = Slug.Generate("Hello World! This Is C# 10 & Beyond!");

            slug.Should().Be("hello-world-this-is-c-10-beyond");
        }

        [Fact]
        public void Slug_Generate_ShouldHandleEmptyAndWhitespace()
        {
            Slug.Generate("").Should().BeEmpty();
            Slug.Generate("   ").Should().BeEmpty();
        }

        [Fact]
        public void PagedResult_PaginationCalculations_ShouldBeAccurate()
        {
            var items = new[] { "item1", "item2" };
            var paged = PagedResult<string>.Create(items, pageNumber: 2, pageSize: 2, totalCount: 5);

            paged.TotalPages.Should().Be(3);
            paged.HasPreviousPage.Should().BeTrue();
            paged.HasNextPage.Should().BeTrue();
        }
    }
}
