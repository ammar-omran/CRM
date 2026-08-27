using CRM.SharedKernel.Domain.Authorization;
using CRM.SharedKernel.Domain.Results;

namespace CRM.Base.Modules.Admin.Permissions;

/// <summary>
/// Thin client used by the Super Admin UI to manage a Portal module's roles/claims.
/// It resolves the module's <c>BaseUrl</c> from the existing <see cref="CRM.Base.Modules.Persistence.ModuleCatalog"/>
/// and calls the module's internal permission endpoints directly over HTTP — no bearer token is required
/// because those endpoints live under the internal network boundary (<c>/internal/permissions</c>), which the
/// public gateway does not expose.
///
/// It also aggregates the permissions exposed by every registered module's manifest into a flat list that the
/// UI uses as the source for cross-module role claims.
/// </summary>
public interface IModulePermissionClient
{
	/// <summary>Returns all roles declared by the given module.</summary>
	Task<Result<RoleSummary[]>> GetRolesAsync(string moduleId, CancellationToken ct = default);

	/// <summary>Returns a single role (with parent/child context) for the given module.</summary>
	Task<Result<RoleDetail>> GetRoleAsync(string moduleId, string roleId, CancellationToken ct = default);

	/// <summary>Returns the claims currently assigned to a role in the given module.</summary>
	Task<Result<RoleClaimModel[]>> GetRoleClaimsAsync(string moduleId, string roleId, CancellationToken ct = default);

	/// <summary>Creates a new role in the given module.</summary>
	Task<Result<string>> CreateRoleAsync(string moduleId, CreateRoleRequest request, CancellationToken ct = default);

	/// <summary>Adds a claim to a role in the given module. Returns the affected role id.</summary>
	Task<Result<string>> AddRoleClaimAsync(string moduleId, string roleId, AddRoleClaimRequest request, CancellationToken ct = default);

	/// <summary>Aggregates every registered module's manifest policies into a flat list of available permissions.</summary>
	Task<IReadOnlyList<ExposedPermission>> GetAvailablePermissionsAsync(CancellationToken ct = default);
}
