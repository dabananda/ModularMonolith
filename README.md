# ModularMonolith

A production-ready, clean architecture **Modular Monolith** starter template and backend built with **ASP.NET Core (.NET 10)**.

This template is designed to serve as a robust, scalable foundation for enterprise applications. Each business module is an independent vertical slice containing its own **Domain**, **Application**, **Infrastructure**, and **Presentation** layers, while sharing a common infrastructure and kernel. When the need arises, any module can be extracted into an independent microservice with minimal friction.

---

## Table of Contents

- [Architecture Overview](#architecture-overview)
- [Solution Structure](#solution-structure)
- [Tech Stack](#tech-stack)
- [Key Features](#key-features)
- [Included Modules](#included-modules)
  - [Identity Module](#identity-module)
  - [Shared Kernel](#shared-kernel)
- [API Endpoints](#api-endpoints)
- [How to Add a New Module](#how-to-add-a-new-module)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Configuration](#configuration)
  - [Database Setup & Migrations](#database-setup--migrations)
  - [Running the Application](#running-the-application)
- [Project Conventions](#project-conventions)

---

## Architecture Overview

The solution strictly enforces **Clean Architecture** and an **inward dependency rule** within each module:

```
Presentation  ──>  Application  ──>  Domain
Infrastructure ──> Application  ──>  Domain
```

- **Domain**: Pure business entities, constants, value objects, and domain logic. Has zero external dependencies.
- **Application**: CQRS Commands, Queries, Handlers, FluentValidation rules, and repository/service interfaces.
- **Infrastructure**: Entity Framework Core DbContext, entity configurations, repository implementations, security services, and external integrations.
- **Presentation**: ASP.NET Core API Controllers and module Dependency Injection registration.

At runtime, the API host project acts as the composition root:

```
+-----------------------------------------------------------------------------+
|                            ModularMonolith.Api                              |
|   (Host: Routing, Middleware, DI Composition Root, Serilog, Rate Limiting)   |
+-------------------------------------+---------------------------------------+
                                      |
                    +-----------------v------------------+
                    |       ModularMonolith.Shared       |
                    |  (CQRS Mediator, Result<T>, Base   |
                    |   Entity, Audit, Cache, MailKit)   |
                    +-----------------+------------------+
                                      |
          +---------------------------+---------------------------+
          |                                                       |
+---------v---------------------------+         +-----------------v-----------+
|          Identity Module            |         |     [Your New Module]       |
|  +-------------+  +---------------+ |         |  +-------------+            |
|  |   Domain    |  |  Application  | |         |  |   Domain    |  ...       |
|  +-------------+  +---------------+ |         |  +-------------+            |
|  +-------------+  +---------------+ |         +-----------------------------+
|  |Infrastructure  | Presentation  | |
|  +-------------+  +---------------+ |
+-------------------------------------+
```

---

## Solution Structure

```
ModularMonolith/
├── ModularMonolith.slnx                                 # Modern .NET 10 solution file
├── ModularMonolith.sln                                  # Classic visual studio solution file
├── README.md
├── .gitignore
│
├── ModularMonolith.Api/                                 # Entry point & Host
│   ├── Middleware/
│   │   └── GlobalExceptionHandler.cs                   # RFC 7807 problem details handler
│   ├── Properties/
│   │   └── launchSettings.json
│   ├── appsettings.json                                 # Central typed configuration
│   ├── appsettings.Development.json
│   ├── ModularMonolith.Api.csproj
│   ├── ModularMonolith.Api.http                         # REST client testing file
│   └── Program.cs                                       # DI composition root & middleware pipeline
│
├── ModularMonolith.Shared/                              # Cross-cutting Shared Kernel
│   ├── Behaviors/
│   │   └── ValidationBehavior.cs                       # Automatic mediator validation pipeline
│   ├── Common/
│   │   ├── BaseController.cs                           # Standard controller with Result<T> mapping
│   │   ├── BaseEntity.cs                               # Auditable entity base class
│   │   ├── ErrorType.cs                                # Structured error classifications
│   │   ├── Result.cs                                   # Functional Result and Result<T> wrappers
│   │   ├── PagedResult.cs                              # Pagination models
│   │   └── Slug.cs                                     # URL slug helper
│   ├── Configurations/
│   │   └── Settings.cs                                 # Strongly typed options with validation
│   ├── Controllers/
│   │   └── ImageController.cs                          # Shared image upload endpoints
│   ├── Entities/
│   │   └── Image.cs                                    # Uploaded media entity
│   ├── Interfaces/
│   │   ├── ICacheService.cs
│   │   ├── ICurrentUserService.cs
│   │   ├── IEmailService.cs
│   │   ├── IImageService.cs
│   │   └── IUnitOfWork.cs
│   ├── Messaging/                                      # Zero-dependency CQRS mediator
│   │   ├── IRequest.cs
│   │   ├── IRequestHandler.cs
│   │   ├── IPipelineBehavior.cs
│   │   ├── ISender.cs
│   │   └── Sender.cs
│   ├── Migrations/
│   ├── Persistence/
│   │   ├── AuditInterceptor.cs                         # Automatic audit trail interceptor
│   │   └── SharedDbContext.cs                          # EF Core context for shared schema
│   ├── Services/
│   │   ├── CloudinaryImageService.cs
│   │   ├── CurrentUserService.cs
│   │   ├── DistributedCacheService.cs
│   │   └── EmailService.cs
│   ├── DependencyInjection.cs
│   └── ModularMonolith.Shared.csproj
│
└── ModularMonolith.Modules.Identity.*                   # Identity & Access Management slice
    ├── ModularMonolith.Modules.Identity.Domain/
    │   ├── Constants/                                  # Roles, schema constants
    │   └── Entities/                                   # User, Role, RefreshToken, VerificationToken
    ├── ModularMonolith.Modules.Identity.Application/
    │   ├── Features/Auth/                              # Register, Login, Refresh, Password Reset, etc.
    │   ├── Interfaces/                                 # Auth & Role repositories, JWT & Hasher contracts
    │   └── DependencyInjection/                        # Automatic handler & validator discovery
    ├── ModularMonolith.Modules.Identity.Infrastructure/
    │   ├── Persistence/                                # ApplicationDbContext ('identity' schema)
    │   ├── Repositories/                               # Auth & Role repository implementations
    │   └── Security/                                   # BCrypt hasher, Secure tokens, JWT generator
    └── ModularMonolith.Modules.Identity.Presentation/
        ├── Controllers/                                # AuthController, RolesController
        └── DependencyInjection.cs                      # AddIdentityModule extension
```

---

## Tech Stack

| Layer / Concern | Technology |
|---|---|
| **Runtime** | .NET 10 (C# 13) |
| **Web Framework** | ASP.NET Core 10 |
| **Data Access / ORM** | Entity Framework Core 10 (SQL Server / PostgreSQL compatible) |
| **Mediator / CQRS** | Built-in zero-dependency Mediator (`ISender`, `IRequest<T>`) |
| **Validation** | FluentValidation 12 with automatic pipeline behaviors |
| **Security & Auth** | JWT Bearer Tokens, Refresh Tokens, BCrypt password hashing |
| **Logging** | Serilog (Structured Console + Daily Rolling File sink) |
| **Caching** | `IDistributedCache` abstraction with in-memory / Redis capability |
| **Mailing** | MailKit (SMTP / TLS) |
| **API Documentation** | Swagger / Swashbuckle OpenAPI |
| **Rate Limiting** | ASP.NET Core Rate Limiting (Partitioned Sliding Window + Fixed Window) |

---

## Key Features

### 1. In-House Zero-Dependency CQRS Mediator
Instead of bringing in heavy external libraries, the solution features an in-house, lightweight mediator (`ISender` and `IRequestHandler<TRequest, TResponse>`). Handlers and pipeline behaviors are automatically scanned and registered into Microsoft DI.

### 2. Functional Result Pattern
Commands and queries return `Result` or `Result<T>` instead of throwing exceptions across layer boundaries:
- `Result.Success(data)`
- `Result.Failure(ErrorType.Validation, "Invalid input")`
- `Result.Failure(ErrorType.NotFound, "Resource not found")`

The `BaseController` automatically translates `ErrorType` into proper HTTP status codes (`400`, `401`, `403`, `404`, `409`, `500`).

### 3. Automatic Validation Pipeline
Every request entering the mediator passes through `ValidationBehavior<TRequest, TResponse>`. If FluentValidation detects validation errors, execution immediately halts and returns a standardized `400 Bad Request` with structured error details.

### 4. Database Schema Isolation
Each module uses its own database schema (e.g. `identity` for Identity, `shared` for Shared, `catalog` for Catalog) while sharing the same underlying database. This guarantees schema-level isolation and makes future database extraction painless.

### 5. Automatic Audit Trail & Soft Delete
All entities deriving from `BaseEntity` are automatically populated with `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`, `DeletedAt`, and `DeletedBy` via EF Core's `AuditSaveChangesInterceptor`. A global query filter automatically enforces soft-deletion (`!e.IsDeleted`).

---

## Included Modules

### Identity Module
Provides complete user authentication and role-based access management:
- **Registration** with email verification token generation
- **Login** with JWT Access Token and persistent Refresh Token
- **Token Refresh** with rotation support
- **Logout** with refresh token revocation
- **Password Reset** (Forgot Password -> Verify Reset Token -> Update Password)
- **Role Management** (Create Role, Assign Role, Remove Role)

### Shared Kernel
Provides cross-cutting utilities and infrastructure:
- Cloudinary media upload service (`IImageService`)
- SMTP email delivery (`IEmailService`)
- Distributed caching (`ICacheService`)
- Current user context resolution from HTTP claims (`ICurrentUserService`)
- Common audit interceptor and base entities

---

## API Endpoints

### Authentication (`/api/v1/auth`)
| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/v1/auth/register` | Register a new user |
| `POST` | `/api/v1/auth/login` | Authenticate and obtain JWT + Refresh token |
| `POST` | `/api/v1/auth/refresh-token` | Exchange refresh token for new access token |
| `POST` | `/api/v1/auth/logout` | Revoke active refresh token |
| `POST` | `/api/v1/auth/verify-email` | Verify user email with token |
| `POST` | `/api/v1/auth/forgot-password` | Request password reset token |
| `POST` | `/api/v1/auth/reset-password` | Set new password using reset token |

### Roles (`/api/v1/roles`)
| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/v1/roles` | List all roles |
| `POST` | `/api/v1/roles` | Create a new role |
| `POST` | `/api/v1/roles/assign` | Assign role to user |
| `POST` | `/api/v1/roles/remove` | Remove role from user |

### Shared Media (`/api/v1/images`)
| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/v1/images/upload` | Upload image to Cloudinary |
| `DELETE` | `/api/v1/images/{id}` | Delete uploaded image |

### System
| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/health` | Health check probe |
| `GET` | `/swagger` | OpenAPI Swagger UI |

---

## How to Add a New Module

Adding a new module (e.g. `Catalog`) is clean and standardized:

### Step 1: Create the 4 Projects
Create a folder for your module (e.g. `ModularMonolith.Modules.Catalog.*`):
```pwsh
# 1. Domain (Class Library, depends only on Shared)
dotnet new classlib -n ModularMonolith.Modules.Catalog.Domain
dotnet add ModularMonolith.Modules.Catalog.Domain reference ModularMonolith.Shared

# 2. Application (Class Library, depends on Domain)
dotnet new classlib -n ModularMonolith.Modules.Catalog.Application
dotnet add ModularMonolith.Modules.Catalog.Application reference ModularMonolith.Modules.Catalog.Domain

# 3. Infrastructure (Class Library, depends on Application & Domain)
dotnet new classlib -n ModularMonolith.Modules.Catalog.Infrastructure
dotnet add ModularMonolith.Modules.Catalog.Infrastructure reference ModularMonolith.Modules.Catalog.Application
dotnet add ModularMonolith.Modules.Catalog.Infrastructure reference ModularMonolith.Modules.Catalog.Domain

# 4. Presentation (Class Library, depends on Application, Infrastructure & Shared)
dotnet new classlib -n ModularMonolith.Modules.Catalog.Presentation
dotnet add ModularMonolith.Modules.Catalog.Presentation reference ModularMonolith.Modules.Catalog.Application
dotnet add ModularMonolith.Modules.Catalog.Presentation reference ModularMonolith.Modules.Catalog.Infrastructure
dotnet add ModularMonolith.Modules.Catalog.Presentation reference ModularMonolith.Shared
```

### Step 2: Add Projects to Solution
```pwsh
dotnet sln ModularMonolith.sln add ModularMonolith.Modules.Catalog.Domain
dotnet sln ModularMonolith.sln add ModularMonolith.Modules.Catalog.Application
dotnet sln ModularMonolith.sln add ModularMonolith.Modules.Catalog.Infrastructure
dotnet sln ModularMonolith.sln add ModularMonolith.Modules.Catalog.Presentation
```

### Step 3: Implement Module DI Registration
In `ModularMonolith.Modules.Catalog.Presentation/DependencyInjection.cs`:
```csharp
namespace ModularMonolith.Modules.Catalog.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddCatalogModule(this IServiceCollection services, Settings settings)
    {
        services.AddCatalogApplication();
        services.AddCatalogInfrastructure(settings);
        return services;
    }
}
```

### Step 4: Register in Host (`ModularMonolith.Api/Program.cs`)
Add project reference to `ModularMonolith.Api`:
```pwsh
dotnet add ModularMonolith.Api reference ModularMonolith.Modules.Catalog.Presentation
```
In `Program.cs`:
```csharp
using ModularMonolith.Modules.Catalog.Presentation;

builder.Services
    .AddSharedProject(settings)
    .AddIdentityModule(settings)
    .AddCatalogModule(settings);
```

---

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server (LocalDB, SQLExpress, or Docker container)

### Configuration
Update connection strings and secrets in `ModularMonolith.Api/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "SqlServerLocal": "Server=localhost\\SQLEXPRESS;Database=ModularMonolithDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "Jwt": {
    "Key": "YourSuperSecretSigningKeyWithSufficientLength123!",
    "Issuer": "ModularMonolith",
    "Audience": "ModularMonolithClient",
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 7
  }
}
```

### Database Setup & Migrations
To generate or apply EF Core migrations:

```pwsh
# Shared context migrations
dotnet ef database update --project ModularMonolith.Shared --startup-project ModularMonolith.Api

# Identity context migrations
dotnet ef migrations add InitialIdentity --project ModularMonolith.Modules.Identity.Infrastructure --startup-project ModularMonolith.Api
dotnet ef database update --project ModularMonolith.Modules.Identity.Infrastructure --startup-project ModularMonolith.Api
```

### Running the Application

```pwsh
dotnet restore ModularMonolith.slnx
dotnet build ModularMonolith.slnx
dotnet run --project ModularMonolith.Api
```

Once running:
- **Swagger UI**: `http://localhost:5259/swagger` or `https://localhost:7213/swagger`
- **Health Check**: `http://localhost:5259/health`

---

## Project Conventions

| Area | Convention | Example |
|---|---|---|
| **Naming** | `ModularMonolith.Modules.<ModuleName>.<Layer>` | `ModularMonolith.Modules.Identity.Application` |
| **Schemas** | Each module owns an isolated DB schema | `identity`, `shared` |
| **CQRS** | In-house mediator commands and queries | `LoginCommand : IRequest<Result<AuthResponse>>` |
| **Validation** | FluentValidation rules paired with commands | `RegisterCommandValidator : AbstractValidator<RegisterCommand>` |
| **Controllers** | Inherit from `BaseController` using `HandleResult()` | `return HandleResult(await sender.Send(command));` |
