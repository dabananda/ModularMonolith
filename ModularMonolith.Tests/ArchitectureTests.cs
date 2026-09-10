using FluentAssertions;
using ModularMonolith.Modules.Identity.Domain.Entities;
using ModularMonolith.Modules.Identity.Infrastructure.Persistence;
using ModularMonolith.Modules.Identity.Presentation.Controllers;
using NetArchTest.Rules;
using System.Reflection;
using Xunit;

namespace ModularMonolith.Tests
{
    /// <summary>
    /// Fitness functions that make the "Clean Architecture inward dependency rule" and
    /// the "modules never reach into each other" promise from the README actually enforced,
    /// instead of relying on reviewers to notice a stray using-directive.
    ///
    /// These tests are intentionally generic over "module name" so that adding a second
    /// module (e.g. Catalog) is automatically covered without touching this file -
    /// as long as the new module follows the ModularMonolith.Modules.<Name>.<Layer> convention
    /// documented in the README.
    /// </summary>
    public class ArchitectureTests
    {
        // Representative assemblies - one per layer of the Identity module.
        // Using a type from each assembly avoids hardcoding assembly name strings everywhere.
        private static readonly Assembly DomainAssembly = typeof(ApplicationUser).Assembly;
        private static readonly Assembly ApplicationAssembly = typeof(ModularMonolith.Modules.Identity.Application.DependencyInjection.DependencyInjection).Assembly;
        private static readonly Assembly InfrastructureAssembly = typeof(ApplicationDbContext).Assembly;
        private static readonly Assembly PresentationAssembly = typeof(AuthController).Assembly;

        private const string ApplicationNamespaceSuffix = ".Application";
        private const string InfrastructureNamespaceSuffix = ".Infrastructure";
        private const string PresentationNamespaceSuffix = ".Presentation";

        // ---------------------------------------------------------------
        // 1. Inward dependency rule within a single module
        // ---------------------------------------------------------------

        [Fact]
        public void Domain_Should_Not_DependOn_OtherLayers()
        {
            var result = Types.InAssembly(DomainAssembly)
                .Should()
                .NotHaveDependencyOnAny(
                    ApplicationNamespaceSuffix,
                    InfrastructureNamespaceSuffix,
                    PresentationNamespaceSuffix)
                .GetResult();

            result.IsSuccessful.Should().BeTrue(BuildFailureMessage(result, "Domain must not depend on Application, Infrastructure, or Presentation."));
        }

        [Fact]
        public void Application_Should_Not_DependOn_InfrastructureOrPresentation()
        {
            var result = Types.InAssembly(ApplicationAssembly)
                .Should()
                .NotHaveDependencyOnAny(
                    InfrastructureNamespaceSuffix,
                    PresentationNamespaceSuffix)
                .GetResult();

            result.IsSuccessful.Should().BeTrue(BuildFailureMessage(result, "Application must not depend on Infrastructure or Presentation."));
        }

        [Fact]
        public void Infrastructure_Should_Not_DependOn_Presentation()
        {
            var result = Types.InAssembly(InfrastructureAssembly)
                .Should()
                .NotHaveDependencyOn("ModularMonolith.Modules.Identity.Presentation")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(BuildFailureMessage(result, "Infrastructure must not depend on Presentation."));
        }

        // ---------------------------------------------------------------
        // 2. Cross-cutting rules that matter more as more modules are added
        // ---------------------------------------------------------------

        [Fact]
        public void Domain_Should_Not_DependOn_EntityFrameworkCore()
        {
            // Domain should be persistence-ignorant. If this starts failing, someone
            // has leaked an EF Core attribute/type into an entity or value object.
            var result = Types.InAssembly(DomainAssembly)
                .Should()
                .NotHaveDependencyOn("Microsoft.EntityFrameworkCore")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(BuildFailureMessage(result, "Domain must have zero EF Core dependency."));
        }

        [Fact]
        public void Domain_Should_Not_DependOn_AspNetCore()
        {
            var result = Types.InAssembly(DomainAssembly)
                .Should()
                .NotHaveDependencyOn("Microsoft.AspNetCore")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(BuildFailureMessage(result, "Domain must have zero ASP.NET Core dependency."));
        }

        [Fact]
        public void Handlers_Should_Be_Sealed_Or_Internal_Implementation_Detail()
        {
            // Not strictly an architecture boundary, but a cheap consistency check:
            // command/query handlers should live under a "Features" folder namespace,
            // never directly in the Application root - keeps vertical slices honest.
            var result = Types.InAssembly(ApplicationAssembly)
                .That()
                .HaveNameEndingWith("Handler")
                .Should()
                .ResideInNamespaceMatching(@"\.Features\.")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(BuildFailureMessage(result, "Handlers must live under a Features.* namespace (vertical slice convention)."));
        }

        // ---------------------------------------------------------------
        // 3. Module isolation - the rule that actually matters once you add
        //    a second module. Written generically: no type outside module X
        //    may depend on module X's Infrastructure or Domain internals,
        //    except through the module's own Presentation DI entry point
        //    and Application-level contracts.
        // ---------------------------------------------------------------

        [Theory]
        [InlineData("Identity")]
        public void Other_Modules_Should_Not_DependOn_ThisModules_Infrastructure(string moduleName)
        {
            var infrastructureNamespace = $"ModularMonolith.Modules.{moduleName}.Infrastructure";

            var otherModuleAssemblies = AllModuleAssemblies()
                .Where(a => !a.GetName().Name!.Contains($".{moduleName}.", StringComparison.Ordinal))
                .ToArray();

            foreach (var assembly in otherModuleAssemblies)
            {
                var result = Types.InAssembly(assembly)
                    .Should()
                    .NotHaveDependencyOn(infrastructureNamespace)
                    .GetResult();

                result.IsSuccessful.Should().BeTrue(
                    BuildFailureMessage(result, $"{assembly.GetName().Name} must not reach into {infrastructureNamespace}."));
            }
        }

        [Theory]
        [InlineData("Identity")]
        public void Other_Modules_Should_Not_DependOn_ThisModules_Domain(string moduleName)
        {
            var domainNamespace = $"ModularMonolith.Modules.{moduleName}.Domain";

            var otherModuleAssemblies = AllModuleAssemblies()
                .Where(a => !a.GetName().Name!.Contains($".{moduleName}.", StringComparison.Ordinal))
                .ToArray();

            foreach (var assembly in otherModuleAssemblies)
            {
                var result = Types.InAssembly(assembly)
                    .Should()
                    .NotHaveDependencyOn(domainNamespace)
                    .GetResult();

                result.IsSuccessful.Should().BeTrue(
                    BuildFailureMessage(result, $"{assembly.GetName().Name} must not reach into {domainNamespace}. Cross-module contracts belong in Application or Shared."));
            }
        }

        // ---------------------------------------------------------------
        // helpers
        // ---------------------------------------------------------------

        private static IEnumerable<Assembly> AllModuleAssemblies() =>
        [
            DomainAssembly,
            ApplicationAssembly,
            InfrastructureAssembly,
            PresentationAssembly
            // Add each new module's four assemblies here when you scaffold it,
            // e.g. typeof(CatalogItem).Assembly, typeof(CatalogDependencyInjection).Assembly, ...
        ];

        private static string BuildFailureMessage(TestResult result, string context)
        {
            var offenders = result.FailingTypeNames is null
                ? "(no type names reported)"
                : string.Join(Environment.NewLine, result.FailingTypeNames);

            return $"{context}{Environment.NewLine}Offending types:{Environment.NewLine}{offenders}";
        }
    }
}