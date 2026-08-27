using CRM.Base.Modules.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRM.Base.Pages.Admin;

/// <summary>
/// Platform-level module management page (the "Settings" entry point).
/// Thin page model: it receives the request, calls the existing application service,
/// maps the result, and renders. No module lifecycle / validation / YARP logic lives here.
/// </summary>
public class IndexModel : PageModel
{
	private readonly IModuleAdminService _admin;

	public IndexModel(IModuleAdminService admin)
	{
		_admin = admin;
	}

	public IReadOnlyList<ModuleSummary> Modules { get; set; } = [];

	public string? StatusMessage { get; set; }
	public bool StatusIsError { get; set; }

	[BindProperty]
	public string ModuleUrl { get; set; } = string.Empty;

	public void OnGet()
	{
		Modules = _admin.ListModules();
	}

	public async Task<IActionResult> OnPostRegisterAsync()
	{
		if (string.IsNullOrWhiteSpace(ModuleUrl))
		{
			StatusMessage = "Module URL is required.";
			StatusIsError = true;
		}
		else
		{
			var result = await _admin.RegisterModuleAsync(ModuleUrl);
			StatusMessage = result.Message;
			StatusIsError = !result.IsSuccess;

			if (!result.IsSuccess && result.Errors.Count > 0)
			{
				StatusMessage += " " + string.Join(" ", result.Errors);
			}
		}

		Modules = _admin.ListModules();
		return Page();
	}

	public async Task<IActionResult> OnPostEnableAsync(string moduleId)
	{
		var result = await _admin.EnableModuleAsync(moduleId);
		StatusMessage = result.Message;
		StatusIsError = !result.IsSuccess;
		Modules = _admin.ListModules();
		return Page();
	}

	public async Task<IActionResult> OnPostDisableAsync(string moduleId)
	{
		var result = await _admin.DisableModuleAsync(moduleId);
		StatusMessage = result.Message;
		StatusIsError = !result.IsSuccess;
		Modules = _admin.ListModules();
		return Page();
	}

	public async Task<IActionResult> OnPostRemoveAsync(string moduleId)
	{
		var result = await _admin.RemoveModuleAsync(moduleId);
		StatusMessage = result.Message;
		StatusIsError = !result.IsSuccess;
		Modules = _admin.ListModules();
		return Page();
	}
}
