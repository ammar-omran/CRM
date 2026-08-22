# Copilot Instructions for CRM Repository

## Build, Test, and Lint

### Building the Project
```bash
# Build the entire solution
dotnet build

# Build a specific project
dotnet build CRM.Base/CRM.Base.csproj
dotnet build Modules/Users/Modules.Users.Features/Modules.Users.Features.csproj

# Build in Release mode
dotnet build --configuration Release
```

### Running Tests
```bash
# Run all tests
dotnet test

# Run tests for a specific module
dotnet test Modules/Customers/Modules.Customers.Test/Modules.Customers.Test.csproj

# Run tests with code coverage
dotnet test /p:CollectCoverage=true

# Run a single test by name (xUnit)
dotnet test --filter "FullyQualifiedName~TestClassName.TestMethodName"
```

### Local Development
```bash
# Run with Aspire orchestrator (recommended for local dev)
# This requires the dotnet aspire workload: dotnet workload restore
dotnet run --project CRM.Host/CRM.Host.csproj

# Run CRM.Base directly (starts the API on https://localhost:5001)
dotnet run --project CRM.Base/CRM.Base.csproj
```

### Code Quality
```bash
# EditorConfig formatting is enforced (see .editorconfig for rules)
# No separate linter configuration exists; use your IDE's built-in formatting
# C# 4-space indentation, 2-space for other file types
```

## High-Level Architecture

### Modular Monolith with DDD

This is a **Domain-Driven Design** system organized as a modular monolith:

#### Core Components

1. **CRM.Base** - API Gateway & Module Host
   - Entry point for all HTTP traffic
   - Uses YARP reverse proxy to route requests to module APIs
   - Manages module registration and lifecycle
   - Provides JWT authentication for all modules
   - Hosts a SQLite platform database for module metadata
   - Swagger/OpenAPI documentation at `/swagger/ui`

2. **CRM.Host** - Development Orchestrator
   - .NET Aspire distributed application orchestrator
   - Runs Users, Customers, Ticketing modules and CRM.Base with service discovery
   - Only for local development; not deployed to production

3. **Modules** (Business Logic)
   - Each module is independently deployable but run within the monolith locally
   - Modules: `Users` (authentication, RBAC), `Customers` (customer management), `Ticketing` (ticket management)
   - Each module declares capabilities, dependencies, routes, policies, and events in a ModuleManifest

4. **SharedKernel** - Cross-Module Abstractions
   - Domain abstractions: `Result<T>` error handling, `IHandler`, `IRepository`, domain events
   - Application abstractions: `IApiEndpoint` for route mapping, `IModuleManifest` for module declarations
   - Infrastructure abstractions: database context base classes, event publishing

### Module Structure

Each module follows a layered architecture:
- **Domain**: Aggregates, entities, domain logic, domain errors, module manifest, policies
- **Features**: Use case handlers, endpoints, validators, shared DTOs (request/response models)
- **Infrastructure**: EF Core DbContext, repository implementations, external service integrations
- **API**: Minimal endpoints that implement `IApiEndpoint` for route registration

Example: `Modules/Users/`
```
Modules.Users.Domain/          # Aggregates, entities, ModuleManifest, policies
Modules.Users.Features/        # Handlers, endpoints, validators
Modules.Users.Infrastructure/  # DbContext, repositories
Modules.Users.API/             # API endpoint registration (minimal project)
```

### Module Registration & Discovery

- Each module publishes a `ModuleManifest` (implements `IModuleManifest`)
- On startup, CRM.Base discovers and validates all modules
- Manifests declare: ID, version, capabilities, dependencies, route prefixes, policies, database schema, events
- YARP is dynamically configured to route `/api/{moduleName}/*` requests to each module's API

### Event System

- Modules can publish and subscribe to events (see `ModuleManifest.Events`)
- Example: Users module publishes `users.organization-created`; Ticketing subscribes to it
- Events are envelope-wrapped and include module context

## Key Conventions

### Error Handling: Result<T> Pattern

All business logic returns `Result<TValue>` or `Result<Success>` instead of throwing exceptions:

```csharp
// Domain errors defined in static Error classes
public static class UserErrors {
    public static Error NotFound(string userId) =>
        Error.NotFound($"Users.{nameof(NotFound)}", $"User with ID {userId} not found");
    
    public static Error InvalidCredentials() =>
        Error.Validation($"Users.{nameof(InvalidCredentials)}", "Invalid email or password");
}

// Handlers return Result<T>
public Task<Result<UserResponse>> HandleAsync(string userId, CancellationToken cancellationToken)
{
    var user = await context.Users.FirstOrDefaultAsync(...);
    if (user is null) {
        return UserErrors.NotFound(userId);  // Implicit conversion
    }
    return new UserResponse(user.Id, user.Email);
}

// Endpoints convert Result to HTTP responses
var response = await handler.HandleAsync(userId, cancellationToken);
if (response.IsError) {
    return response.Errors.ToProblem();  // Returns ProblemDetails
}
return Results.Ok(response.Value);
```

**Important**: Error codes must use the format `{ModulePrefix}.{ErrorType}` (e.g., `Users.NotFound`, `Customers.ValidationError`).

### Feature Organization

Each use case lives in a dedicated feature folder with:
- `{FeatureName}.Endpoint.cs` - Implements `IApiEndpoint`, maps HTTP route
- `{FeatureName}.Handler.cs` - Implements `IHandler<T>`, contains business logic
- `{FeatureName}.Validators.cs` - FluentValidation validators (if needed)
- `{FeatureName}.Request/Response.cs` - DTOs
- Shared/ folder with common models and errors for the feature group

Example: `Features/Users/GetUserById/`
```
GetUserById.Endpoint.cs     # Maps GET /api/users/{id}
GetUserById.Handler.cs      # Queries DbContext, returns UserResponse
GetUserById.Request.cs      # (if needed)
Shared/UserResponse.cs      # DTO used across User features
```

### Policy-Based Authorization

Modules define policies in their manifest. Endpoints enforce them:

```csharp
// Manifest declares policy
public IReadOnlyList<ModulePolicy> Policies => [
    new ModulePolicy { Name = "users:read", Description = "..." },
    new ModulePolicy { Name = "users:create", Description = "..." }
];

// Endpoint requires policy
app.MapGet(route, handler)
    .RequireAuthorization(UserPolicyConsts.ReadPolicy);  // Built from manifest
```

Policies are enforced at the endpoint level via ASP.NET Core authorization middleware.

### Database Schema Isolation

Each module has its own database schema (SQLite allows multiple schemas in one database):
- Users module: `users` schema
- Customers module: `customers` schema
- Ticketing module: `ticketing` schema

DbContext classes are module-specific (e.g., `UsersDbContext`).

### Dependency Injection

Each module registers its dependencies in its API project. The pattern:
1. Module features depend on `CRM.SharedKernel.Application`
2. Infrastructure layer implements repositories and db contexts
3. API project wires everything together and makes endpoints discoverable

CRM.Base calls `app.MapModuleEndpoints()` to discover and register all `IApiEndpoint` implementations.

### Testing

- Uses **xUnit** for test framework
- Uses **Moq** for mocking
- Uses **coverlet** for code coverage
- Test projects follow naming: `Modules.{ModuleName}.Test`
- No existing test files; follow handler/endpoint testing conventions

## Running CRM.Base in Different Environments

```bash
# Development (uses .editorconfig defaults, SQLite in-memory or local file)
dotnet run --project CRM.Base/CRM.Base.csproj

# Development with Aspire (recommended; manages dependent services)
dotnet run --project CRM.Host/CRM.Host.csproj

# Production build
dotnet publish --configuration Release
```

## Common Workflows

### Adding a New Feature to a Module

1. Create feature folder in `Modules/{ModuleName}/Modules.{ModuleName}.Features/Features/{FeatureGroup}/{FeatureName}`
2. Create handler: `{FeatureName}.Handler.cs` - implements `IHandler`, returns `Result<T>`
3. Create endpoint: `{FeatureName}.Endpoint.cs` - implements `IApiEndpoint`, maps HTTP route
4. Add validators if needed: `{FeatureName}.Validators.cs`
5. Use error classes from Domain for error handling
6. Declare any new policies in the ModuleManifest if authorization is needed

### Adding a New Module

1. Create module directory structure under `Modules/{ModuleName}`
2. Create `Modules.{ModuleName}.Domain` → define aggregates, entities, ModuleManifest, errors
3. Create `Modules.{ModuleName}.Features` → implement features (handlers + endpoints)
4. Create `Modules.{ModuleName}.Infrastructure` → EF Core DbContext, repositories
5. Create `Modules.{ModuleName}.API` → minimal project that exposes endpoints
6. Update `CRM.Host/Program.cs` to include the new module in orchestration
7. Update `CRM.Base` project references to the new module (or use dynamic discovery)
8. Ensure ModuleManifest declares all routes, capabilities, policies

### Debugging Modules

- Breakpoints work across module boundaries
- Use `dotnet run` or Aspire orchestrator to run locally
- Check module registration logs on CRM.Base startup to verify manifest loading
- SQLite database file is in `CRM.Base/platform.db`; inspect with any SQLite viewer

### Updating Dependencies

- Shared across modules via `SharedKernel` projects
- Individual module dependencies updated in respective `.csproj` files
- .NET SDK version: net10.0 (latest)

## Code Style

- **Indentation**: 4 spaces for C#, 2 spaces for JSON/YAML/Markdown (enforced by .editorconfig)
- **Nullability**: `#nullable enable` is set project-wide; use nullable reference types
- **Usings**: System directives first, sorted alphabetically within groups
- **Naming**: PascalCase for types/methods, camelCase for parameters/locals
- **Error Codes**: Must follow `{ModulePrefix}.{ErrorType}` format (e.g., `Users.NotFound`)

## References

- **ASP.NET Core**: https://docs.microsoft.com/aspnet/core
- **Entity Framework Core**: https://docs.microsoft.com/ef/core
- **FluentValidation**: https://docs.fluentvalidation.net
- **xUnit**: https://xunit.net
- **YARP**: https://microsoft.github.io/reverse-proxy/
- **Aspire**: https://learn.microsoft.com/dotnet/aspire
