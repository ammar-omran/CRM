using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using CRM.SharedKernel.Domain.Authorization;
using CRM.SharedKernel.Domain.Results;
using CRM.SharedKernel.Application.API.Extensions;

namespace CRM.SharedKernel.Application.API.Authorization;

/// <summary>
/// Maps the default Portal-module permission endpoints under <c>/internal/permissions</c>
/// on top of an <see cref="IRolePermissionStore"/> implementation.
///
/// These endpoints deliberately live under the <c>/internal</c> boundary (the same
/// convention used by <c>/internal/events</c>): they are platform-only, are NOT exposed
/// through the public module gateway, and are only reachable from the local network
/// (e.g. the Super Admin host calling a module directly via its <c>BaseUrl</c>). Because
/// access is gated by the internal network / gateway boundary rather than a per-user
/// bearer claim, no authorization policy is applied here.
///
/// A Portal module calls this once from its host:
/// <c>app.MapPortalPermissionEndpoints();</c>.
/// The store is resolved per-request from DI, so multiple Portal modules can each mount
/// their own endpoints without ambiguity.
/// </summary>
public static class PortalPermissionEndpoints
{
	/// <summary>
	/// Maps the standard role/permission endpoints under <c>/internal/permissions</c>.
	/// </summary>
	/// <param name="app">The web application.</param>
	public static WebApplication MapPortalPermissionEndpoints(this WebApplication app)
	{
		ArgumentNullException.ThrowIfNull(app);

		const string root = "/internal/permissions";

		app.MapGet($"{root}/roles",
				async (IRolePermissionStore store, CancellationToken ct) =>
					Results.Ok(await store.GetRolesAsync(ct)));

		app.MapGet($"{root}/roles/{{roleId}}",
				async (string roleId, IRolePermissionStore store, CancellationToken ct) =>
				{
					var result = await store.GetRoleAsync(roleId, ct);
					return result.IsSuccess ? Results.Ok(result.Value) : result.Errors.ToProblem();
				});

		app.MapPost($"{root}/roles",
				async (CreateRoleRequest request, IRolePermissionStore store, CancellationToken ct) =>
				{
					var result = await store.CreateRoleAsync(request, ct);
					return result.IsSuccess
						? Results.Created($"{root}/roles/{result.Value}", result.Value)
						: result.Errors.ToProblem();
				});

		app.MapGet($"{root}/roles/{{roleId}}/claims",
				async (string roleId, IRolePermissionStore store, CancellationToken ct) =>
				{
					var result = await store.GetRoleClaimsAsync(roleId, ct);
					return result.IsSuccess ? Results.Ok(result.Value) : result.Errors.ToProblem();
				});

		app.MapPost($"{root}/roles/{{roleId}}/claims",
				async (string roleId, AddRoleClaimRequest request, IRolePermissionStore store, CancellationToken ct) =>
				{
					var result = await store.AddRoleClaimAsync(roleId, request, ct);
					return result.IsSuccess ? Results.Ok(result.Value) : result.Errors.ToProblem();
				});

		return app;
	}
}
