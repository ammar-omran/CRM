using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CRM.SharedKernel.Domain.Authorization;
using CRM.SharedKernel.Domain.Results;
using Modules.Customers.Infrastructure.Database;
using Modules.Customers.Domain.Entities;

namespace Modules.Customers.Infrastructure.Authorization;

/// <summary>
/// Default <see cref="IRolePermissionStore"/> for the Users (Portal) module.
/// Backed by ASP.NET Core Identity (<see cref="RoleManager{Role}"/>) and the
/// <see cref="CustomersDbContext"/>. This is the single owner of the module's role data;
/// the SharedKernel permission endpoints are the only public surface over it.
/// </summary>
internal sealed class CustomersRolePermissionStore : IRolePermissionStore
{
	private readonly RoleManager<CustomerRole> _roleManager;
	private readonly CustomersDbContext _db;

	public CustomersRolePermissionStore(RoleManager<CustomerRole> roleManager, CustomersDbContext db)
	{
		_roleManager = roleManager;
		_db = db;
	}

	/// <inheritdoc />
	public async Task<IReadOnlyList<RoleSummary>> GetRolesAsync(CancellationToken ct = default)
	{
		var roles = await _db.Roles
			.OrderBy(r => r.Name)
			.Select(r => new RoleSummary(
				r.Id,
				r.Name ?? string.Empty,
				null, null, 0, _db.RoleClaims.Count(rc => rc.RoleId == r.Id)))
			.ToListAsync(ct);

		return roles;
	}

	/// <inheritdoc />
	public async Task<Result<RoleDetail>> GetRoleAsync(string roleId, CancellationToken ct = default)
	{
		var role = await _db.Roles
			.Include(r => r.RoleClaims)
			.AsSplitQuery()
			.FirstOrDefaultAsync(r => r.Id == roleId, ct);

		if (role is null)
		{
			return Error.NotFound("Role.NotFound", $"Role '{roleId}' was not found.");
		}

		var claims = role.RoleClaims
			.Select(rc => new RoleClaimModel(rc.Id.ToString(), rc.ClaimType ?? string.Empty, rc.ClaimValue ?? string.Empty))
			.ToList();

		return new RoleDetail(
			role.Id,
			role.Name ?? string.Empty,
			null, null,
			claims,
			[]);
	}

	/// <inheritdoc />
	public async Task<Result<IReadOnlyList<RoleClaimModel>>> GetRoleClaimsAsync(string roleId, CancellationToken ct = default)
	{
		if (!await _db.Roles.AnyAsync(r => r.Id == roleId, ct))
		{
			return Error.NotFound("Role.NotFound", $"Role '{roleId}' was not found.");
		}

		var claims = await _db.RoleClaims
			.Where(rc => rc.RoleId == roleId)
			.Select(rc => new RoleClaimModel(rc.Id.ToString(), rc.ClaimType ?? string.Empty, rc.ClaimValue ?? string.Empty))
			.ToListAsync(ct);

		return claims;
	}

	/// <inheritdoc />
	public async Task<Result<string>> CreateRoleAsync(CreateRoleRequest request, CancellationToken ct = default)
	{
		if (string.IsNullOrWhiteSpace(request.Name))
		{
			return Error.Validation("Role.Name.Required", "Role name is required.");
		}

		if (await _roleManager.RoleExistsAsync(request.Name))
		{
			return Error.Conflict("Role.Exists", $"A role named '{request.Name}' already exists.");
		}

		if (request.ParentRoleId is not null)
		{
			var parent = await _roleManager.FindByIdAsync(request.ParentRoleId);
			if (parent is null)
			{
				return Error.NotFound("Role.Parent.NotFound", $"Parent role '{request.ParentRoleId}' was not found.");
			}
		}

		var role = new CustomerRole
		{
			Id = Guid.NewGuid().ToString(),
			Name = request.Name,
		};

		var identityResult = await _roleManager.CreateAsync(role);
		if (!identityResult.Succeeded)
		{
			return Error.Failure("Role.Create.Failed",
				string.Join("; ", identityResult.Errors.Select(e => e.Description)));
		}

		return role.Id;
	}

	/// <inheritdoc />
	public async Task<Result<string>> AddRoleClaimAsync(string roleId, AddRoleClaimRequest request, CancellationToken ct = default)
	{
		if (string.IsNullOrWhiteSpace(request.ClaimType))
		{
			return Error.Validation("RoleClaim.ClaimType.Required", "Claim type is required.");
		}

		var role = await _roleManager.FindByIdAsync(roleId);
		if (role is null)
		{
			return Error.NotFound("Role.NotFound", $"Role '{roleId}' was not found.");
		}

		var existing = await _roleManager.GetClaimsAsync(role);
		if (existing.Any(c =>
				string.Equals(c.Type, request.ClaimType, StringComparison.OrdinalIgnoreCase) &&
				string.Equals(c.Value, request.ClaimValue, StringComparison.OrdinalIgnoreCase)))
		{
			return role.Id; // already present — idempotent
		}

		var identityResult = await _roleManager.AddClaimAsync(
			role, new Claim(request.ClaimType, request.ClaimValue ?? string.Empty));

		if (!identityResult.Succeeded)
		{
			return Error.Failure("RoleClaim.Add.Failed",
				string.Join("; ", identityResult.Errors.Select(e => e.Description)));
		}

		return role.Id;
	}
}
