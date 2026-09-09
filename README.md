# Azka CRM

Modular monolith CRM built on ASP.NET Core, YARP, EF Core + SQL Server (platform DB on SQLite), ASP.NET Identity + JWT, and a vertical-slice `SharedKernel` + module architecture. The platform (`CRM.Base` / `CRM.Host`) discovers, routes, and orchestrates isolated modules via a runtime module catalog and internal module-event bus.

## Objectives

- Isolate business capabilities into independently versionable modules (`Users`, `Customers`, `Tickets`) with their own schema, policies, and event contracts.
- Keep cross-module coupling loose: modules communicate through the platform's `/internal/events/publish` → `PlatformEventDispatcher` → `/internal/events` bus, not direct DB access.
- Enforce consistent patterns: `Result<T>` + `Error`, `IHandler` handlers, `TicketOperator` value-object roles, multi-role `CurrentUser`, pagination/filtering, and audit history as strong DTOs.

## Architecture

```
CRM.Host          # Aspire host
CRM.Base          # Platform: YARP reverse proxy, Razor Super-Admin (/), Swagger, module catalog, internal events
SharedKernel      # Domain (Results, Events, Modules, IAuditable), Application (handlers, BaseResponse, Pagination, tracing), Infrastructure (EF, Auth, Email, Policies)
Modules/
  Users           # Portal: Identity, RBAC, Organizations + Agents + Customers (Org DB)
  Customers       # Customer identity, OTP, status
  Tickets         # Ticketing: tickets, operators, history, lookups (Ticketing DB)
```

- **Module manifest** (`IModuleManifest`) declares `ModuleId`, `RoutePrefixs`, `Policies` (from `*PolicyConstants`), `Events.Published/Subscribed`, `Database.Schema`, capabilities. Policies are auto-registered via `IPolicyFactory` + `AuthorizationConfigureOptions` as `permission` claim policies (`crm.*:*`).
- **Routing:** `CRM.Base` maps `MapModuleEndpoints()` + `MapInternalEndpoints()` + YARP. Each module registers `AddCoreInfrastructure` (JWT, `ICurrentUserService`, `IEmailSender`) and `RegisterHandlersFromAssemblyContaining`.
- **Events:** `IModuleEventPublisher.PublishAsync(TEvent)` → envelope `POST /internal/events/publish` → dispatcher fans out to subscribers' `POST /internal/events` by `EventName` (e.g. `ticketing.ticket-created`). Handlers derive from `ModuleEventHandler<TEvent>`.

## Domain Conventions (Ticketing)

- **Auth:** `CurrentUser` carries `UserId, Name, Email, Roles: IReadOnlyList<string>, RolesCsv, TokenPayload (JwtPayload), RawToken`. `CurrentUserService` aggregates all `ClaimTypes.Role` + `role` claims. Legacy `Role` (first) kept for compat.
- **TicketOperator role** — persisted as `"{operation}:{userCsv}"` (e.g. `creator:Admin;Support`, `assignee:Agent`):
  - `TicketOperationRole` enum `Creator/Assignee/Owner/Watcher`
  - `TicketOperator.Role` (256) with `OperatorRole`, `UserRoles`, `OperationRoleEnum`, `SetRole(TicketOperationRole, IEnumerable<string>)` + legacy raw fallback
  - **Ideal ownership:** `Operator.UserRolesCsv` (512) snapshots user roles at last seen; `TicketOperator` preserves role at assignment time for audit.
  - `Ticket.Create(creatorUserRoles, Creator)` + `AssignOperator(Assignee, userRoles)` use it.
- **Ticket isolation:** `Ticket.GroupId` = `payload[GroupKey]` (`appsettings GroupKey=OrganizationId`). Lists are policy-gated:
  - `GET /api/tickets/list` (`view-all`) — unscoped
  - `GET /api/tickets/mine` (`view-mine`) — `TicketOperators → Operator.RefId == UserId`
  - `GET /api/tickets/group` (`view-group`) — `ticket.GroupId == payload[GroupKey]` (missing claim → empty page, not 403)
  - `GET /api/tickets/{id}` + `/{id}/history` → `view-any` OR-gate (`view-all|view-mine|view-group|view-any`) + data-level check (full viewer or assigned/same-group, else 403 `Ticket.UnauthorizedAccess`).
- **History:** `TicketHistoryResponse(Id, FieldName, OldValue, NewValue, OperatorId, OperatorName, CreatedDate)` — raw field changes, no `TicketAuditHelper` formatting; frontend localizes. Ordered newest-first.
- **Pagination/Filtering:** `TicketFilterRequest : PaginationRequest` (`Title?, StatusIds[], SeverityIds[], CategoryIds[], TypeIds[], From/ToDate`) + `ApplyFilters()` helper; `PaginationResponse<T>` with `SkipTotal` support.

## Tech Stack

.NET 8/10, ASP.NET Core, EF Core (SQL Server + SQLite platform DB), ASP.NET Identity, JWT Bearer, YARP, Swagger/OpenAPI, SmtpEmailSender, Serilog, Humanizer/FluentValidation, Aspire (Host).

## Getting Started

### Prerequisites

- .NET SDK 8+ (repo builds on 10)
- SQL Server (or Docker) — `ConnectionStrings:UsersDb`, `CustomersDb`, `TicketingDb`
- SMTP (MailServer) or leave empty for no-op in dev

### Configuration

Each module `appsettings.json` needs:

```json
{
  "ConnectionStrings": { "UsersDb": "Server=...,1433;Database=CRM-Users;...", "TicketingDb": "..." },
  "AuthConfiguration": { "Key": "AzkaUserManagement@SecretKey#2025!CRM", "Issuer": "UserManagementAPI", "Audience": "CRMPortalClient" },
  "MailServer": { "Server": "smtp.gmail.com", "Port": 587, "Username": "", "Password": "", "EnableSsl": true, "FromAddress": "", "FromName": "CRM" },
  "InternalEvents": { "PlatformBaseUrl": "https://localhost:5001" },
  "GroupKey": "OrganizationId",
  "ServiceUrls": { "CustomerManagementApi": "https://localhost:5001/", "TicketingApi": "https://localhost:5002/" }
}
```

### Run

```bash
dotnet restore
dotnet run --project CRM.Base        # platform + Super Admin at /
dotnet run --project CRM.Host        # Aspire orchestrator (if using)
dotnet run --project Modules/Users/Modules.Users.API
dotnet run --project Modules/Tickets/Modules.Ticketing.API
dotnet run --project Modules/Customers/Modules.Customers.API
```

On startup each module runs `Database.MigrateAsync()` (e.g. `20261001_AddOperatorUserRoles` adds `operators.user_roles_csv`). Platform DB (`CRM.Base/platform.db`) is auto-created/migrated.

Swagger: `/swagger` per module + `/swagger` on `CRM.Base`.

Auth: `POST /api/users/login` → JWT with `permission` claims (`crm.ticketing:ticket:*`). Attach `Authorization: Bearer <token>`.

## API Reference (Ticketing)

- `POST /api/tickets` (`create`) — `CreateTicketRequest(OtherTitle?, Description, CategoryId, TypeId, TitleId?, SeverityId?)` → `CreateTicketResponse`; auto-resolves `SeverityId ?? Title.DefaultSeverityId`, group from `payload[GroupKey]`, creates `Operator` snapshot, publishes `ticketing.ticket-created(TicketId, GroupId, Title, Description)`, best-effort email.
- `GET /api/tickets/list|/mine|/group?Title=&StatusIds=&SeverityIds=&CategoryIds=&TypeIds=&FromDate=&ToDate=&Skip=&Limit=&SkipTotal=` — paginated lists
- `GET /api/tickets/{id}` — `TicketDetailsResponse` (ids+names, `Operators[]` with formatted `Role`)
- `GET /api/tickets/{id}/history` — `TicketHistoryResponse[]`
- `POST /api/tickets/{id}/assign` (`assign`) — `AssignTicketRequest(AssigneeRefId, AssigneeName?, AssigneeEmail?, Role: TicketOperationRole=Assignee)` → `AssignTicketResponse`
- `GET /api/tickets/lookups/{severities|categories|types|services|statuses}` + `GET /api/tickets/lookups/titles?categoryId=` — `LookupItemResponse` / `TicketTitleLookupResponse` (visible `Sort→Name`)

All errors return `BaseResponse` via `ToMVCProblem()` (400/401/403/404/409 by `ErrorType`).

## Cross-Module Workflow

`Ticketing` → `Users.TicketCreatedHandler` (`ticketing.ticket-created`):

1. `GroupId` → `int organizationId` → load `Organization` + `OrganizationAgents → Agent → User/Role`.
2. For each agent with email, render `TicketNotificationTemplates.NewTicketEmail` and `IEmailSender.SendAsync` (subject `New ticket #id — {Org}`), per-agent try/catch.
3. Explicit assignment left to `POST /assign` (keeps modules loosely coupled; auto-assign via `HttpClient → ServiceUrls:TicketingApi` can be added without changing contracts).

## Build and Test

```bash
dotnet build
dotnet test
dotnet ef migrations add <Name> -p Modules/Tickets/Modules.Ticketing.Infrastructure -s Modules/Tickets/Modules.Ticketing.API
dotnet ef database update -p Modules/Tickets/Modules.Ticketing.Infrastructure -s Modules/Tickets/Modules.Ticketing.API
```

## Contribute

- Follow vertical-slice: ` Domain (entities/value objects) → Features (IHandler + Request/Response) → Infrastructure (EF configs/migrations) → API (controllers)`.
- Keep `SharedKernel` contracts stable; add policies via `*PolicyConstants` + manifest `FromConstants`.
- Write handlers as pure `Result<T>` with `TicketErrors`/`Error`; use `IModuleEventPublisher` for cross-module, `IEventPublisher` for in-module.
- Add migrations for any `Operator`/`TicketOperator` schema change; keep `TicketOperator.Role` formatted and `Operator.UserRolesCsv` in sync.

## License

Internal — Azka.
