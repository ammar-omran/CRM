using System.ComponentModel.DataAnnotations;
using CRM.Base.Modules.Admin;
using CRM.Base.Modules.Admin.Permissions;
using CRM.SharedKernel.Domain.Authorization;
using CRM.SharedKernel.Domain.Modules;
using CRM.SharedKernel.Domain.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRM.Base.Pages.Admin.Modules;

/// <summary>
/// Module permission management page (Portal modules only).
/// Reads/writes a module's roles and claims through the existing <see cref="IModulePermissionClient"/>,
/// which targets the module's internal endpoints directly. Contains no role/permission logic of its own.
/// </summary>
public class PermissionsModel : PageModel
{
	private readonly IModuleAdminService _admin;
	private readonly IModulePermissionClient _client;

	public PermissionsModel(IModuleAdminService admin, IModulePermissionClient client)
	{
		_admin = admin;
		_client = client;
	}

	public ModuleDetail? Module { get; set; }
	public IReadOnlyList<RoleSummary> Roles { get; set; } = Array.Empty<RoleSummary>();
	public RoleSummary? SelectedRole { get; set; }
	public IReadOnlyList<RoleClaimModel> Claims { get; set; } = Array.Empty<RoleClaimModel>();
	public IReadOnlyList<ExposedPermission> AvailablePermissions { get; set; } = Array.Empty<ExposedPermission>();
	public string? ErrorMessage { get; set; }
	public string? SuccessMessage { get; set; }

	// Not [BindProperty]: each POST handler binds only its own model via TryUpdateModelAsync,
	// so submitting one form does not trigger validation of the other form's [Required] fields.
	public AddClaimBinding AddClaim { get; set; } = new();
	public CreateRoleBinding CreateRole { get; set; } = new();

	public async Task<IActionResult> OnGetAsync(string moduleId, string? roleId, CancellationToken cancellationToken = default)
	{
		Module = _admin.GetModule(moduleId);
		if (Module is null) return NotFound();
		if (Module.Kind != ModuleKind.Portal) return NotFound();

		await LoadAsync(moduleId, roleId, cancellationToken);
		return Page();
	}

	public async Task<IActionResult> OnPostAddClaimAsync(CancellationToken cancellationToken)
	{
		var addClaim = AddClaim;
		await TryUpdateModelAsync(addClaim, nameof(AddClaim));

		Module = _admin.GetModule(addClaim.ModuleId);
		if (Module is null || Module.Kind != ModuleKind.Portal) return NotFound();

		if (!ModelState.IsValid)
		{
			await LoadAsync(addClaim.ModuleId, addClaim.RoleId, cancellationToken);
			return Page();
		}

		var request = new AddRoleClaimRequest(addClaim.ClaimType, addClaim.ClaimValue);
		var result = await _client.AddRoleClaimAsync(addClaim.ModuleId, addClaim.RoleId, request, cancellationToken);

		if (result.IsError)
			ErrorMessage = FormatErrors(result.Errors);
		else
			SuccessMessage = $"Added claim '{addClaim.ClaimType}' to role.";

		await LoadAsync(addClaim.ModuleId, addClaim.RoleId, cancellationToken);
		return Page();
	}

	public async Task<IActionResult> OnPostCreateRoleAsync(CancellationToken cancellationToken)
	{
		var createRole = CreateRole;
		await TryUpdateModelAsync(createRole, nameof(CreateRole));

		Module = _admin.GetModule(createRole.ModuleId);
		if (Module is null || Module.Kind != ModuleKind.Portal) return NotFound();

		if (!ModelState.IsValid)
		{
			await LoadAsync(createRole.ModuleId, null, cancellationToken);
			return Page();
		}

		var request = new CreateRoleRequest(createRole.Name, createRole.ParentRoleId);
		var result = await _client.CreateRoleAsync(createRole.ModuleId, request, cancellationToken);

		if (result.IsError)
			ErrorMessage = FormatErrors(result.Errors);
		else
			SuccessMessage = $"Created role '{createRole.Name}'.";

		var focusRoleId = result.IsSuccess ? result.Value : null;
		await LoadAsync(createRole.ModuleId, focusRoleId, cancellationToken);
		return Page();
	}

	private async Task LoadAsync(string moduleId, string? roleId, CancellationToken ct)
	{
		AddClaim.ModuleId = moduleId;
		CreateRole.ModuleId = moduleId;

		var rolesResult = await _client.GetRolesAsync(moduleId, ct);
		if (rolesResult.IsError)
			ErrorMessage = FormatErrors(rolesResult.Errors);
		else
			Roles = rolesResult.Value ?? Array.Empty<RoleSummary>();

		if (!string.IsNullOrWhiteSpace(roleId) && rolesResult.IsSuccess)
		{
			var claimsResult = await _client.GetRoleClaimsAsync(moduleId, roleId, ct);
			if (claimsResult.IsError)
				ErrorMessage = FormatErrors(claimsResult.Errors);
			else
			{
				Claims = claimsResult.Value ?? Array.Empty<RoleClaimModel>();
				SelectedRole = Roles.FirstOrDefault(r => r.Id == roleId);
			}

			AddClaim.RoleId = roleId;
		}

		AvailablePermissions = await _client.GetAvailablePermissionsAsync(ct);
	}

	private static string FormatErrors(List<Error> errors) =>
		string.Join(" ", errors.Select(e => e.Description));
}

public sealed class AddClaimBinding
{
	public string ModuleId { get; set; } = "";
	public string RoleId { get; set; } = "";
	[Required] public string ClaimType { get; set; } = PermissionClaims.Type;
	[Required] public string ClaimValue { get; set; } = "";
}

public sealed class CreateRoleBinding
{
	public string ModuleId { get; set; } = "";
	[Required] public string Name { get; set; } = "";
	public string? ParentRoleId { get; set; }
}
