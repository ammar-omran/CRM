using CRM.SharedKernel.Domain.Results;

namespace CRM.SharedKernel.Domain.Authorization;

/// <summary>
/// Foundation contracts for Portal-module role & permission management.
/// These are Identity-agnostic: a Portal module (e.g. Users) implements
/// <see cref="IRolePermissionStore"/> against its own role store, and the
/// SharedKernel-provided endpoint mapper exposes a standard, uniform API.
/// </summary>

/// <summary>A lightweight summary of a role for list views.</summary>
public sealed record RoleSummary(
	string Id,
	string Name,
	string? ParentRoleId,
	string? ParentRoleName,
	int ChildRoleCount,
	int ClaimCount);

/// <summary>A single claim attached to a role (e.g. an exposed module permission).</summary>
public sealed record RoleClaimModel(
	string Id,
	string ClaimType,
	string ClaimValue);

/// <summary>Full detail of a role, including its claims and direct child roles.</summary>
public sealed record RoleDetail(
	string Id,
	string Name,
	string? ParentRoleId,
	string? ParentRoleName,
	IReadOnlyList<RoleClaimModel> Claims,
	IReadOnlyList<RoleSummary> ChildRoles);

/// <summary>Request to create a new role, optionally nested under a parent role.</summary>
public sealed record CreateRoleRequest(
	string Name,
	string? ParentRoleId);

/// <summary>Request to attach a claim (permission) to a role.</summary>
public sealed record AddRoleClaimRequest(
	string ClaimType,
	string ClaimValue);

/// <summary>
/// Abstraction a Portal module implements to read and write its roles and role claims.
/// The store is the single owner of the module's RBAC data; the platform and the
/// Super Admin UI are consumers of the endpoints built on top of it.
/// </summary>
public interface IRolePermissionStore
{
	/// <summary>Returns all roles with their parent, child-count, and claim-count.</summary>
	Task<IReadOnlyList<RoleSummary>> GetRolesAsync(CancellationToken cancellationToken = default);

	/// <summary>Returns a single role with its claims and child roles, or a NotFound error.</summary>
	Task<Result<RoleDetail>> GetRoleAsync(string roleId, CancellationToken cancellationToken = default);

	/// <summary>Returns the claims of a role, or a NotFound error if the role is missing.</summary>
	Task<Result<IReadOnlyList<RoleClaimModel>>> GetRoleClaimsAsync(string roleId, CancellationToken cancellationToken = default);

	/// <summary>Creates a role. Returns the new role id, or a Validation/Conflict/NotFound error.</summary>
	Task<Result<string>> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken = default);

	/// <summary>Adds a claim to a role (idempotent). Returns the role id, or a Validation/NotFound error.</summary>
	Task<Result<string>> AddRoleClaimAsync(string roleId, AddRoleClaimRequest request, CancellationToken cancellationToken = default);
}
