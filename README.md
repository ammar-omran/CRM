# Azka CRM

Modular monolith CRM on **ASP.NET Core 10, YARP, EF Core + SQL Server, ASP.NET Identity + JWT, OpenTelemetry**. The platform (`CRM.Base` / `CRM.Host`) discovers, routes and orchestrates isolated vertical-slice modules via a runtime module catalog and internal event bus.

```
CRM.Host          # Aspire orchestrator
CRM.Base          # Platform: YARP reverse proxy, Razor Super-Admin (/), Swagger, ModuleCatalog, /internal/* 
SharedKernel      # Domain (Result<T>, Error, Events, IModuleManifest, IAuditable) 
                  # Application (IHandler, BaseResponse<T>, Pagination, Tracing, ModuleEventPublisher)
                  # Infrastructure (EF AuditableInterceptor, JWT Auth, CurrentUserService, Policies, SmtpEmailSender)
Modules/
  Users           # Portal workforce: Identity (UsersDbContext: users), Organizations/Agents (OrganizationsDbContext: org)
  Customers       # External portal: Identity (CustomersDbContext: customers), OTP activation
  Tickets         # Helpdesk: TicketingDbContext (ticketing)
```

## Objectives

* Isolate capabilities into independently versionable modules with own **schema, policies, migrations and event contracts**.
* Loose coupling via `IModuleEventPublisher → POST /internal/events/publish → PlatformEventDispatcher → POST /internal/events` (e.g. `ticketing.ticket-created`), not direct DB access.
* Consistent patterns: `Result<T>`+`Error` → `ToMVCProblem()`/`ToProblem()` → `BaseResponse`, `IHandler`, `FluentValidation`, `PaginationRequest/Response`, `ControllerBase` + `ProducesResponseType(typeof(BaseResponse),400/409)` for OpenAPI.

---

## Architecture

* **Module manifest** (`IModuleManifest`) declares `ModuleId`, `DisplayName`, `RoutePrefixs` (`/api/users`, `/api/agents`, `/api/organizations`, `/api/customers`, `/api/tickets`), `AnonymousPaths` (login/register/refresh/set-password/confirm-email...), `Policies` auto-registered via `IPolicyFactory` + `AuthorizationConfigureOptions` as `RequireClaim("permission", "crm.*")`, `Events.Published/Subscribed`, `Database.SchemaName`, `Health`.
* **Routing:** `CRM.Base` → `MapModuleEndpoints` + `MapInternalEndpoints` + YARP. Each module registers `AddCoreInfrastructure` (JWT `AuthConfiguration` + `TokenValidationParameters`, `ICurrentUserService`, `IEmailSender`, `IPasswordHasher`) and `RegisterHandlersFromAssemblyContaining` + `AddValidatorsFromAssembly`.
* **API style:** migrated from Minimal API (`IApiEndpoint`) to **MVC Controllers** (`Modules.Users.API` / `Modules.Customers.API` / `Modules.Ticketing.API` – like `TicketsController`) with `[ApiController]`, `[Produces("application/json")]`, XML `<summary>` for Swagger.
* **Auth:** `CurrentUserService` aggregates `HttpContext.User` → `CurrentUser(UserId, Name, Email, Role, RawToken, TokenPayload)` using standard claims: `NameIdentifier/Sub` → `UserId`, `ClaimTypes.Name` → `Name`, `ClaimTypes.Email` → `Email`, `ClaimTypes.Role` → `Role` (fallback to legacy `userid`/`role`/`sub`). `JwtTokenGenerator` removed – generation now directly in `ClientAuthorizationService`/`CustomerAuthorizationService`.
* **Events:** `IEventPublisher` (in-module) and `IModuleEventPublisher` (cross-module) publish envelopes; handlers implement `IEventHandler<T>` / `ModuleEventHandler<T>`.

---

## Modules

### Users (`crm.users` – Portal, internal audience)

**Schemas:** `users` (`UsersDbContext : IdentityDbContext<User,Role,string,...>`) + `org` (`OrganizationsDbContext : DbContext`)

**Identity:** `User : IdentityUser` (`IsActive=false` by default, `CreatedAt/UpdatedAt`, `Claims/UserRoles/Logins/Tokens`), `Role : IdentityRole` (seed `Agent,Supervisor,Admin` + child `TeamLead/FirstLine/SecondLine` under `Agent`), `Customer`-like `RefreshToken` (`Token/PK, JwtId, UserId, ExpiryDate, Invalidated`). Password & lockout via `IdentityOptions` (`RequireDigit/Upper/Lower/NonAlpha/8`, `AllowedForNewUsers=true, MaxFailed=5, Lockout 15m`). `LockoutEnabled` per-user enables brute-force protection (`AccessFailedCount`/`LockoutEnd`); `IsActive` gates login before `CheckPasswordSignInAsync`.

**Agent** `org.Agents` – **refactored** to `UserId:string FK → users.Users.Id` (cross-schema via `UserReferenceConfiguration : ExcludeFromMigrations`, like `AgentRoleConfiguration` for `OrganizationAgents.AgentRoleId → users.Roles`). `Name`/`Email` columns removed – data lives in `User`. Config `AgentsConfiguration` (`HasOne User, HasIndex UserId`). Migrations edited in-place (no new migration).

**Organization** `org.Organizations` + `OrganizationAgent` (M:N `Agent–Organization` with `AgentRoleId → users.Roles`) + `Customer` ( `org.Customers` linked to external Customers API via `ICustomerRepository`).

**Auth flow:**
- `POST /api/users/register` (`RegisterUserRequest: Email, Name, Password? , Phone?, Role?`) – `Password` optional (forced `SetPassword`), `Phone` optional (stored `PhoneNumber`). `UserManager.CreateAsync` (with/without password), inactive, optional role assignment → `UserResponse`.
- `POST /api/users/set-password` (`SetPasswordRequest: Token, Password`) – validates `IPasswordResetTokenGenerator` payload, expiry, `IsActive` guard, strong-password (`PasswordValidator`), hashes via `UserManager.PasswordHasher`, activates (`IsActive=true, EmailConfirmed=true`).
- `POST /api/users/login` – checks `IsActive`, `CheckPasswordSignInAsync(..., lockoutOnFailure:true)` → `IsLockedOut` → 401, otherwise issues JWT (30m) + `CustomerRefreshToken` (7d) via `GenerateJwtToken` (`NameIdentifier/Sub/Name/Email/Role` + `RoleClaims` walking parent chain + `UserClaims`). Implements `LoginUserResponse(Token, RefreshToken)`.
- `POST /api/users/refresh` – validates `TokenValidationParameters` (lifetime false), `Jti`, `RefreshToken` existence/expiry/`Invalidated`/`JwtId` match, re-issues.
- `POST /api/users/current-organization` (`SetCurrentOrganizationRequest: OrganizationId`) `[Authorize]` – validates membership via `Agents.Where(UserId==userId)` → `OrganizationAgents`, upserts `UserClaim("OrganizationId")`, syncs agent sub-roles (`4,5,6` → `UserRoles`) and invalidates refresh tokens (memory cache `RoleChanged`).

**Controllers** (`Users.API`):
- `UsersController` (`/api/users`): `login`/`register`/`refresh`/`set-password` (`AllowAnonymous`), `GET/DELETE /{userId}` (`Read/Delete` policies), `POST /current-organization` (`[Authorize]`)
- `AgentsController` (`/api/agents`): `POST /` (`AgentCreatePolicy`), `GET /` (`AgentReadPolicy`) – `Agents`/`Organizations` renamed from singular to avoid entity namespace collision, `IRepository<T>` replaced with direct `OrganizationsDbContext` (`AsQueryable/CountAsync/Select/ToListAsync`, `Include(Agent.User)`)
- `OrganizationsController` (`/api/organizations`): `POST /`, `GET /`, `GET /customers` (all `AllowAnonymous` currently) – direct `DbContext` usage

All actions return `BaseResponse` for `400/409` (`ProducesResponseType(typeof(BaseResponse),400/409)`) and domain `LoginUserResponse`, `UserResponse`, `OrganizationResponse`, `AgentResponse(UserId/Name/Email)`.

**Policies:** `UserPolicyConstants` (`user:read/create/update/delete`, `agent:read/create/update/delete`) auto-registered via `UsersPolicyFactory`.

**Migrations:** edited in-place `20260822_InitOrg` + snapshot (remove `RoleId` shadow FK `ChildRoles` bug → `Invalid column name 'RoleId'` fix by `Ignore(r=>r.ChildRoles)`).

### Customers (`crm.customers` – External portal)

Mirrored on Users standard with **OTP activation** and **default `Customer` role**.

**Schema:** `customers` (`CustomersDbContext : IdentityDbContext<Customer,CustomerRole,string,...>`)

**Identity:** `Customer : IdentityUser` (`Name`, owned `PhoneNumber(Number/CountryCode)`, `IsActive/IsEmailVerified/IsLocked`, `OTP/HashedEmail/OTPCreatedDate/FailedAttempts`, `CreatedAt/UpdatedAt`, `Claims/UserRoles/...`), `CustomerRole : IdentityRole` (seed `Customer`), `CustomerRefreshToken` (like Users).

**Auth flow (`CustomerAuthorizationService`):**
- `POST /api/customers/register` (`Name, Email, PhoneNumber, CountryCode, Password`) – checks unique email/phone, `UserManager.CreateAsync` (inactive), generate 6-digit OTP + `SHA256` short-hash `HashedEmail`, store `OTP/HashedEmail/OTPCreatedDate`, send email (`OTPEXPIRATION` default 5m), return `RegisterCustomerResponse(HashedEmail, MaskedEmail)`.
- `POST /api/customers/confirm-email` (`HashedEmail, Otp`) – checks `IsLocked`, expiry, `OTP` mismatch → increment `FailedAttempts` (lock after 3 → `IsLocked`), success → `IsEmailVerified/IsActive/EmailConfirmed`, clear OTP, assign `Customer` role.
- `POST /api/customers/resend-otp` (`HashedEmail`) – cooldown `ResendCooldown` (3m), regenerate OTP.
- `POST /api/customers/login` (`Email, Password`) – checks `IsActive/IsEmailVerified`, `CheckPasswordSignInAsync(lockoutOnFailure:true)` → `LockedOut`, else JWT (`NameIdentifier/Sub/Name/Email/Role` + `Customer` default) + refresh.
- `POST /api/customers/refresh` – same Jti/refresh validation as Users.

**Controllers** (`Customers.API`): `CustomersController` (`/api/customers`) – `register/confirm-email/resend-otp/login/refresh` (`AllowAnonymous`), `GET /` + `GET /{customerId}` (`ViewPolicy`), all `BaseResponse` 400/409.

**Manifest:** `AnonymousPaths: login, login-phone, register, confirm-email, resend-otp, refresh, update-password, reset-password`.

### Tickets (`crm.tickets` – Helpdesk)

`CurrentUser(UserId, Name, Email, Roles, RawToken, TokenPayload)` snapshot at request.

`TicketOperator.Role = "{operation}:{userCsv}"` (`TicketOperationRole: Creator/Assignee/Owner/Watcher`, `OperatorRole`, `UserRolesCsv`), `Ticket.GroupId = payload[GroupKey]` (`OrganizationId`).

Paginated lists: `GET /api/tickets/list` (`view-all`), `/mine` (`view-mine` → `Operator.RefId==UserId`), `/group` (`view-group` → `GroupId==payload[GroupKey]`), `GET /{id}`/`/{id}/history` (`view-any` OR-gate), history as `TicketHistoryResponse`.

Cross-module: `Ticketing → Users.TicketCreatedHandler` (`ticketing.ticket-created`) emails agents per `OrganizationAgents`.

---

## Tech Stack

.NET 10, ASP.NET Core, EF Core (SQL Server + SQLite `platform.db`), ASP.NET Identity, JWT Bearer, YARP, Swagger/OpenAPI, SmtpEmailSender, BCrypt, FluentValidation, OpenTelemetry, Aspire.

## Getting Started

**Prereqs:** .NET SDK 10, SQL Server (or Docker), SMTP `MailServer` or no-op in dev.

**Configuration** (per module `appsettings.json`):

```json
{
  "ConnectionStrings": { "UsersDb": "Server=...;Database=CRM-Users;...", "CustomerManagementDb": "...", "TicketingDb": "..." },
  "AuthConfiguration": { "Key": "AzkaUserManagement@SecretKey#2025!CRM", "Issuer": "UserManagementAPI", "Audience": "CRMPortalClient" },
  "MailServer": { "Server": "smtp.gmail.com", "Port": 587, "Username": "", "Password": "", "EnableSsl": true, "FromAddress": "", "FromName": "CRM" },
  "InternalEvents": { "PlatformBaseUrl": "https://localhost:5001" },
  "GroupKey": "OrganizationId",
  "OTPEXPIRATION": 5,
  "ResendCooldown": 3,
  "ServiceUrls": { "CustomerManagementApi": "https://localhost:5001/", "TicketingApi": "https://localhost:5002/" }
}
```

**Run:**

```bash
dotnet restore
dotnet run --project CRM.Base        # platform + Super Admin at /
dotnet run --project CRM.Host        # Aspire orchestrator
dotnet run --project Modules/Users/Modules.Users.API
dotnet run --project Modules/Customers/Modules.Customers.API
dotnet run --project Modules/Tickets/Modules.Ticketing.API
```

Each module runs `IModuleDatabaseMigrator.MigrateAsync()` on start (`users`, `org`, `customers`, `ticketing`). Swagger at `/swagger` per module + `/swagger` on `CRM.Base`. Auth: `POST /api/users/login` or `/api/customers/login` → `Authorization: Bearer <token>` with `permission` claims (`crm.users:*`, `crm.customers:*`, `crm.ticketing:*`).

## API Reference

### Users (`/api/users`, `/api/agents`, `/api/organizations`)

| Method | Route | Auth | Request | Response |
|--------|-------|------|---------|----------|
| POST | `/api/users/login` | Anon | `LoginUserRequest(Email,Password)` | `LoginUserResponse(Token,RefreshToken)` |
| POST | `/api/users/register` | Anon | `RegisterUserRequest(Email,Name,Password?,Phone?,Role?)` | `UserResponse(Id,Email)` (201) |
| POST | `/api/users/refresh` | Anon | `RefreshTokenRequest(Token,RefreshToken)` | `RefreshTokenResponse` |
| POST | `/api/users/set-password` | Anon | `SetPasswordRequest(Token,Password)` | 204 |
| GET | `/api/users/{userId}` | `user:read` | - | `UserResponse` |
| DELETE | `/api/users/{userId}` | `user:delete` | - | 204 |
| POST | `/api/users/current-organization` | Auth | `SetCurrentOrganizationRequest(OrganizationId)` | 204 (sets `OrganizationId` claim + sub-role) |
| POST | `/api/agents` | `agent:create` | `AddAgentRequest(UserId)` | `AgentResponse(Id,UserId,Name,Email)` |
| GET | `/api/agents?UserId=&Skip=&Limit=` | `agent:read` | `AgentByRequest` | `PaginationResponse<AgentResponse>` |
| POST | `/api/organizations` | Anon | `AddOrganizationRequest(Name,Agents[{Id,Role}],Customers[{Id}])` | `OrganizationResponse` |
| GET | `/api/organizations?Name=&Skip=&Limit=` | Anon | `GetOrganizationRequest` | `PaginationResponse<OrganizationResponse>` |
| GET | `/api/organizations/customers?Name=&Email=&OrganizationName=&Skip=&Limit=` | Anon | `GetOrganizationCustomersRequest` | `PaginationResponse<OrganizationCustomerResponse>` |

### Customers (`/api/customers`)

All `BaseResponse` for `400/409`.

| Method | Route | Auth | Request |
|--------|-------|------|---------|
| POST | `/api/customers/register` | Anon | `RegisterRequest(Name,Email,PhoneNumber,CountryCode,Password)` → `RegisterResponse(HashedEmail,MaskedEmail)` |
| POST | `/api/customers/confirm-email` | Anon | `ConfirmEmailRequest(HashedEmail,Otp)` → 204 (activates, assigns `Customer`) |
| POST | `/api/customers/resend-otp` | Anon | `ResendOtpRequest(HashedEmail)` → 204 |
| POST | `/api/customers/login` | Anon | `LoginRequest(Email,Password)` → `LoginResponse(Token,RefreshToken)` |
| POST | `/api/customers/refresh` | Anon | `RefreshTokenRequest(Token,RefreshToken)` → `CustomerRefreshTokenResponse` |
| GET | `/api/customers?Skip=&Limit=` | `customers:view` | `PaginationResponse<CustomerResponse>` |
| GET | `/api/customers/{customerId}` | `customers:view` | `CustomerResponse` |

### Tickets

`POST /api/tickets` (`create`) → `CreateTicketResponse`, `GET /api/tickets/list|mine|group`, `GET /{id}`, `GET /{id}/history`, `POST /{id}/assign`, `GET /lookups/*` – all `BaseResponse` via `ToMVCProblem()`.

## Build & Test

```bash
dotnet build
dotnet build CRM.Host/CRM.Host.csproj -c Release
dotnet ef migrations add <Name> -p Modules/Users/Modules.Users.Infrastructure -s Modules/Users/Modules.Users.API --context UsersDbContext
dotnet ef migrations add <Name> -p Modules/Customers/Modules.Customers.Infrastructure -s Modules/Customers/Modules.Customers.API --context CustomersDbContext
dotnet ef database update --context UsersDbContext
```

## Contribute

Vertical-slice: `Domain (entities/value objects) → Features (IHandler + Request/Response/Validator) → Infrastructure (EF Configurations/Migrations, Authorization) → API (Controllers)`. Keep `SharedKernel` contracts stable; add policies via `*PolicyConstants` + `Manifest.FromConstants`. Handlers return `Result<T>` with `*Errors`; use `IModuleEventPublisher` cross-module, `IEventPublisher` in-module. Prefer direct `DbContext` over `IRepository<T>` (refactored). Edit migrations in-place only when re-seeding; otherwise add new migration.

## License

Internal — Azka.
