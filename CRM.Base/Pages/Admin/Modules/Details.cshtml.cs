using CRM.Base.Modules.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRM.Base.Pages.Admin.Modules;

/// <summary>
/// Generic module administration / details page.
/// Reads manifest + registration information through the existing application service.
/// Establishes navigation/structure for future configuration; does not implement it.
/// </summary>
public class DetailsModel : PageModel
{
	private readonly IModuleAdminService _admin;

	public DetailsModel(IModuleAdminService admin)
	{
		_admin = admin;
	}

	public ModuleDetail? Module { get; set; }

	public string? RequestedModuleId { get; set; }

	public IActionResult OnGet(string moduleId)
	{
		RequestedModuleId = moduleId;
		Module = _admin.GetModule(moduleId);

		if (Module is null)
		{
			return NotFound();
		}

		return Page();
	}
}
