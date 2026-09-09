# Contributing to ModularMonolith

Thank you for your interest in contributing to **ModularMonolith**! We welcome contributions from everyone, whether you are fixing a bug, adding new module slices, improving test coverage, or enhancing documentation.

This guide outlines our architecture principles, coding standards, development setup, and pull request workflow to help you contribute effectively.

---

## Table of Contents

1. [Code of Conduct](#code-of-conduct)
2. [Architectural Guardrails](#architectural-guardrails)
   - [Clean Architecture & Inward Dependency](#clean-architecture--inward-dependency)
   - [Module Autonomy & Isolation](#module-autonomy--isolation)
   - [Database Schema Isolation](#database-schema-isolation)
   - [CQRS & Functional Result Pattern](#cqrs--functional-result-pattern)
   - [Automatic Validation Pipeline](#automatic-validation-pipeline)
3. [Development Setup](#development-setup)
   - [Prerequisites](#prerequisites)
   - [Getting Started](#getting-started)
   - [Database Migrations](#database-migrations)
4. [Coding Standards & Rules](#coding-standards--rules)
   - [Zero Compiler Warnings Policy](#zero-compiler-warnings-policy)
   - [Nullable Reference Types](#nullable-reference-types)
   - [C# 13 & Modern .NET Conventions](#c-13--modern-net-conventions)
   - [Code Formatting](#code-formatting)
5. [Adding a New Module](#adding-a-new-module)
6. [Testing Guidelines](#testing-guidelines)
7. [Git & Commit Conventions](#git--commit-conventions)
8. [Pull Request Process & Checklist](#pull-request-process--checklist)
9. [Reporting Issues & Getting Help](#reporting-issues--getting-help)

---

## Code of Conduct

We are committed to providing a welcoming, respectful, and inclusive community for all contributors.

- **Be respectful and collaborative**: Encourage constructive discussions and thoughtful feedback.
- **Focus on quality**: Strive to produce clean, maintainable, and well-tested code.
- **Open communication**: Ask questions early if you are unsure about architectural boundaries or design patterns.

---

## Architectural Guardrails

To keep the codebase maintainable, scalable, and readily extractable into microservices if needed, every contribution **must strictly adhere** to the following design principles:

### Clean Architecture & Inward Dependency

Each business module is structured into four distinct vertical layers with a strict inward dependency flow:

```
Presentation  ──>  Application  ──>  Domain
Infrastructure ──> Application  ──>  Domain
```

- **Domain Layer (`*.Domain`)**:
  - Contains entities, value objects, domain logic, and module-specific constants.
  - **Zero external dependencies**: Must not reference EF Core, presentation libraries, HTTP contexts, or other application/infrastructure layers.
- **Application Layer (`*.Application`)**:
  - Contains CQRS commands, queries, handlers, interfaces, and FluentValidation validators.
  - Depends **only** on the `Domain` layer and `ModularMonolith.Shared`.
- **Infrastructure Layer (`*.Infrastructure`)**:
  - Contains EF Core `DbContext`, entity type configurations, repository implementations, and external integrations (e.g., hashers, security tokens).
  - Depends on `Application` and `Domain`.
- **Presentation Layer (`*.Presentation`)**:
  - Contains ASP.NET Core API controllers and module-level dependency injection registration (`Add<Module>Module`).
  - Inherits from `BaseController`.

### Module Autonomy & Isolation

- **No Direct Cross-Module References**: Modules must **never** reference another module's `Domain`, `Application`, or `Infrastructure` assemblies.
- **Inter-Module Communication**:
  - Use shared abstractions in `ModularMonolith.Shared` or domain/integration events.
  - Avoid tight synchronous coupling between business modules.

### Database Schema Isolation

- Each module manages its own data within a designated database schema (e.g., `identity` for Identity, `catalog` for Catalog, `shared` for Shared).
- Cross-schema foreign keys and direct cross-module EF Core joins are **forbidden**.
- Each module has its own `DbContext` and generates independent EF Core migrations.

### CQRS & Functional Result Pattern

- **Commands & Queries**: Use our zero-dependency mediator (`ISender`, `IRequest<Result>`, `IRequest<Result<T>>`).
- **No Exception-Driven Flow Control**: Handlers return functional `Result` or `Result<T>` instead of throwing business exceptions.
  ```csharp
  // Success
  return Result.Success(data);

  // Failure with standard error classification
  return Result.Failure(ErrorType.NotFound, "User not found.");
  return Result.Failure(ErrorType.Conflict, "Email already exists.");
  ```
- **BaseController Integration**: In controllers, delegate execution to `ISender` and return `HandleResult(...)`:
  ```csharp
  [HttpPost("login")]
  public async Task<IActionResult> Login([FromBody] LoginCommand command)
  {
      var result = await Sender.Send(command);
      return HandleResult(result);
  }
  ```

### Automatic Validation Pipeline

- All request validation rules must be implemented via FluentValidation by inheriting from `AbstractValidator<TRequest>`.
- Validators are placed in the Application layer alongside their respective command/query.
- The mediator pipeline automatically discovers and runs validators via `ValidationBehavior<TRequest, TResponse>`. Do not manually validate models inside handlers or controllers.

---

## Development Setup

### Prerequisites

Ensure you have the following installed on your local machine:

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (`dotnet --version`)
- [SQL Server](https://www.microsoft.com/sql-server) (LocalDB, SQL Express, or Docker container)
- [Git](https://git-scm.com/)
- An IDE of choice:
  - Visual Studio 2022 / 2025 (v17.12+ recommended for .NET 10)
  - JetBrains Rider
  - VS Code with the C# Dev Kit extension

### Getting Started

1. **Fork and clone the repository**:
   ```bash
   git clone https://github.com/<your-username>/ModularMonolith.git
   cd ModularMonolith
   ```

2. **Restore dependencies**:
   ```bash
   dotnet restore ModularMonolith.slnx
   ```

3. **Configure local settings**:
   Ensure `ModularMonolith.Api/appsettings.Development.json` has a valid connection string pointing to your local SQL Server instance.

4. **Build the solution**:
   ```bash
   dotnet build ModularMonolith.slnx
   ```

5. **Run the API**:
   ```bash
   dotnet run --project ModularMonolith.Api
   ```
   Access Swagger UI at `http://localhost:5001/swagger` or `https://localhost:5000/swagger`.

### Database Migrations

When updating entity models, apply or generate migrations through EF Core CLI:

```bash
# Apply Shared migrations
dotnet ef database update --project ModularMonolith.Shared --startup-project ModularMonolith.Api

# Add migration to a module (e.g., Identity)
dotnet ef migrations add <MigrationName> --project ModularMonolith.Modules.Identity.Infrastructure --startup-project ModularMonolith.Api

# Apply Identity migrations
dotnet ef database update --project ModularMonolith.Modules.Identity.Infrastructure --startup-project ModularMonolith.Api
```

---

## Coding Standards & Rules

### Zero Compiler Warnings Policy

The solution is configured via `Directory.Build.props` with:

```xml
<TreatWarningsAsErrors>true</TreatWarningsAsErrors>
<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
<Nullable>enable</Nullable>
```

- **Any compiler warning will fail the build.**
- Unused variables, missing XML doc warnings (where configured), or potential nullability discrepancies must be resolved cleanly before submitting code.

### Nullable Reference Types

- Keep nullability enabled across all projects.
- Avoid unconditional null-forgiving operators (`!`) unless guaranteed by design or framework lifecycle (e.g., EF Core navigation properties with appropriate comments).
- Guard all external inputs and query outputs with explicit null checks or pattern matching.

### C# 13 & Modern .NET Conventions

- Use **file-scoped namespaces**:
  ```csharp
  namespace ModularMonolith.Modules.Identity.Application.Features.Auth;
  ```
- Use **primary constructors** for dependency injection where suitable:
  ```csharp
  public class LoginCommandHandler(IAuthRepository authRepository, IJwtProvider jwtProvider)
      : IRequestHandler<LoginCommand, Result<AuthResponse>>
  {
      ...
  }
  ```
- Use **collection expressions** (`[]`) and modern pattern matching.
- Keep classes `sealed` or `internal` unless intended for extension or cross-assembly public consumption.

### Code Formatting

We use the standard .NET formatting tool. Verify and fix code style before committing:

```bash
# Verify formatting without applying changes (used in CI)
dotnet format ModularMonolith.slnx --verify-no-changes

# Automatically fix formatting issues
dotnet format ModularMonolith.slnx
```

---

## Adding a New Module

When contributing a new business module (e.g., `Catalog`), follow the established convention:

1. **Create 4 projects** under the `ModularMonolith.Modules.<ModuleName>` naming scheme:
   - `ModularMonolith.Modules.<ModuleName>.Domain`
   - `ModularMonolith.Modules.<ModuleName>.Application`
   - `ModularMonolith.Modules.<ModuleName>.Infrastructure`
   - `ModularMonolith.Modules.<ModuleName>.Presentation`
2. **Add projects to the solution** (`ModularMonolith.slnx`).
3. **Configure isolated schema**: Set `modelBuilder.HasDefaultSchema("<module_name>")` in the module's `DbContext`.
4. **Implement DI registration**:
   - `Add<ModuleName>Application()` in Application layer
   - `Add<ModuleName>Infrastructure(Settings settings)` in Infrastructure layer
   - `Add<ModuleName>Module(this IServiceCollection services, Settings settings)` in Presentation layer
5. **Register the module** in `ModularMonolith.Api/Program.cs`.
6. **Add automated tests** in `ModularMonolith.Tests`.

See the [How to Add a New Module](README.md#how-to-add-a-new-module) section in `README.md` for complete command-line walkthroughs.

---

## Testing Guidelines

Quality and testability are first-class citizens in this project. Every new feature, command, query, or bug fix must be covered by automated tests.

- **Test Project**: All automated tests reside in `ModularMonolith.Tests`.
- **Frameworks**: Built using **xUnit**, **FluentAssertions**, and **Moq**.
- **Test Categories**:
  - **Domain Tests**: Test domain entity behavior, invariant enforcement, state transitions, and validation (e.g., `ApplicationUserDomainTests.cs`).
  - **CQRS Handler Tests**: Test command and query handlers in isolation with mocked dependencies (e.g., `AuthFeaturesTests.cs`).
  - **Mediator & Behavior Tests**: Verify pipeline behaviors such as validation and auditing (e.g., `MediatorTests.cs`, `AuditInterceptorTests.cs`).
- **Naming Convention**: Use descriptive test names that express the scenario and expected outcome:
  ```csharp
  public void Handle_WhenEmailAlreadyExists_ShouldReturnConflictResult()
  ```

Run all tests prior to submitting your contribution:

```bash
dotnet test ModularMonolith.slnx
```

Ensure all tests pass with 0 failures and 0 warnings.

---

## Git & Commit Conventions

### Branch Naming

Create short, descriptive branches branched from `master` (or `main`):

- `feature/<short-description>` (e.g., `feature/order-module-slice`)
- `fix/<short-description>` (e.g., `fix/refresh-token-rotation`)
- `refactor/<short-description>` (e.g., `refactor/mediator-pipeline`)
- `docs/<short-description>` (e.g., `docs/contribution-guideline`)
- `chore/<short-description>` (e.g., `chore/bump-nuget-packages`)

### Commit Messages

We follow [Conventional Commits](https://www.conventionalcommits.org/):

- `feat: add order checkout command and validator`
- `fix: correct token expiration calculation in JwtProvider`
- `refactor: extract common paging query extension`
- `test: add unit tests for role assignment handler`
- `docs: update API endpoints table in README`
- `chore: update dependencies to .NET 10 Preview / GA`

---

## Pull Request Process & Checklist

1. **Sync with Upstream**: Make sure your branch is up to date with the latest `master`.
2. **Run Local Checks**: Ensure the entire build, format, and test suite succeed cleanly.
3. **Open a Pull Request**: Provide a clear description of:
   - What problem is solved or what feature is introduced.
   - Any architectural considerations or design trade-offs.
   - Confirmation that automated tests were added or updated.

### Pull Request Checklist

Before marking your PR as ready for review, verify:

- [ ] `dotnet build ModularMonolith.slnx` builds with **0 warnings** and **0 errors**.
- [ ] `dotnet test ModularMonolith.slnx` passes with **100% success**.
- [ ] `dotnet format ModularMonolith.slnx --verify-no-changes` produces **no formatting violations**.
- [ ] Inward Clean Architecture dependency rules are strictly observed.
- [ ] No direct inter-module project dependencies introduced.
- [ ] Entity changes have corresponding EF Core migrations in the module's Infrastructure layer.
- [ ] New features/endpoints include corresponding unit tests in `ModularMonolith.Tests`.
- [ ] Any public configuration options or settings are documented in `README.md`.

---

## Reporting Issues & Getting Help

- **Bug Reports**: If you discover a bug, please open an issue describing the expected behavior, actual behavior, steps to reproduce, and your local environment (.NET version, OS, DB).
- **Feature Requests**: Open an issue or discussion outlining the proposed feature, use case, and architectural approach.
- **Questions**: If you are unsure how to structure a feature or adhere to module boundaries, open a discussion or ask in the relevant issue thread.

Thank you for helping make **ModularMonolith** better!
