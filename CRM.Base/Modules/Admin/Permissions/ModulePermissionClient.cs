using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CRM.Base.Modules.Admin;
using CRM.Base.Modules.Persistence;
using CRM.SharedKernel.Domain.Authorization;
using CRM.SharedKernel.Domain.Results;
using Microsoft.Extensions.Logging;

namespace CRM.Base.Modules.Admin.Permissions;

/// <summary>
/// Default implementation of <see cref="IModulePermissionClient"/>. Pure HTTP orchestration over the
/// existing <see cref="ModuleCatalog"/> — it introduces no module/role logic of its own.
/// </summary>
public sealed class ModulePermissionClient : IModulePermissionClient
{
	private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

	private readonly ModuleCatalog _catalog;
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly ILogger<ModulePermissionClient> _logger;

	public ModulePermissionClient(
		ModuleCatalog catalog,
		IHttpClientFactory httpClientFactory,
		ILogger<ModulePermissionClient> logger)
	{
		_catalog = catalog;
		_httpClientFactory = httpClientFactory;
		_logger = logger;
	}

	private Result<string> ResolveBaseUrl(string moduleId)
	{
		var entry = _catalog.Get(moduleId);
		return entry is null
			? Error.NotFound("Module.NotFound", $"Module '{moduleId}' is not registered.")
			: entry.Entity.BaseUrl;
	}

	public async Task<Result<RoleSummary[]>> GetRolesAsync(string moduleId, CancellationToken ct = default)
	{
		var baseUrl = ResolveBaseUrl(moduleId);
		if (baseUrl.IsError) return baseUrl.Errors;

		var (value, error) = await HttpGetAsync<RoleSummary[]>($"{baseUrl.Value!}/internal/permissions/roles", ct);
		return error is not null ? error : value!;
	}

	public async Task<Result<RoleDetail>> GetRoleAsync(string moduleId, string roleId, CancellationToken ct = default)
	{
		var baseUrl = ResolveBaseUrl(moduleId);
		if (baseUrl.IsError) return baseUrl.Errors;

		var (value, error) = await HttpGetAsync<RoleDetail>($"{baseUrl.Value!}/internal/permissions/roles/{roleId}", ct);
		return error is not null ? error : value!;
	}

	public async Task<Result<RoleClaimModel[]>> GetRoleClaimsAsync(string moduleId, string roleId, CancellationToken ct = default)
	{
		var baseUrl = ResolveBaseUrl(moduleId);
		if (baseUrl.IsError) return baseUrl.Errors;

		var (value, error) = await HttpGetAsync<RoleClaimModel[]>($"{baseUrl.Value!}/internal/permissions/roles/{roleId}/claims", ct);
		return error is not null ? error : value!;
	}

	public async Task<Result<string>> CreateRoleAsync(string moduleId, CreateRoleRequest request, CancellationToken ct = default)
	{
		var baseUrl = ResolveBaseUrl(moduleId);
		if (baseUrl.IsError) return baseUrl.Errors;

		var (value, error) = await HttpPostAsync<CreateRoleRequest, string>(
			$"{baseUrl.Value!}/internal/permissions/roles", request, ct);
		return error is not null ? error : value!;
	}

	public async Task<Result<string>> AddRoleClaimAsync(string moduleId, string roleId, AddRoleClaimRequest request, CancellationToken ct = default)
	{
		var baseUrl = ResolveBaseUrl(moduleId);
		if (baseUrl.IsError) return baseUrl.Errors;

		var (value, error) = await HttpPostAsync<AddRoleClaimRequest, string>(
			$"{baseUrl.Value!}/internal/permissions/roles/{roleId}/claims", request, ct);
		return error is not null ? error : value!;
	}

	public Task<IReadOnlyList<ExposedPermission>> GetAvailablePermissionsAsync(CancellationToken ct = default)
	{
		var list = new List<ExposedPermission>();

		foreach (var entry in _catalog.GetAll())
		{
			var manifest = entry.Manifest;
			if (manifest?.Policies is null) continue;

			foreach (var policy in manifest.Policies)
			{
				list.Add(new ExposedPermission
				{
					SourceModuleId = entry.Entity.ModuleId,
					SourceModuleName = entry.Entity.DisplayName,
					Policy = policy.Name,
					// policy.Name is already the fully-qualified permission (module id prefix applied in the
					// module's own policy constants / manifest), so it is the canonical claim value.
					FullPermission = policy.Name,
					Description = policy.Description
				});
			}
		}

		return Task.FromResult<IReadOnlyList<ExposedPermission>>(list);
	}

	private async Task<(T? Value, List<Error>? Error)> HttpGetAsync<T>(string url, CancellationToken ct)
	{
		try
		{
			var client = _httpClientFactory.CreateClient();
			client.Timeout = TimeSpan.FromSeconds(10);

			using var response = await client.GetAsync(url, ct);
			var body = await response.Content.ReadAsStringAsync(ct);

			if (!response.IsSuccessStatusCode)
			{
				return (default, [Error.Failure("Permission.FetchFailed",
					$"Module returned {(int)response.StatusCode} from {new Uri(url).PathAndQuery}.")]);
			}

			var value = JsonSerializer.Deserialize<T>(body, JsonOptions);
			return value is null
				? (default, [Error.Failure("Permission.Deserialize", "Module returned an empty response.")])
				: (value, null);
		}
		catch (JsonException ex)
		{
			_logger.LogWarning(ex, "Failed to deserialize response from {Url}", url);
			return (default, [Error.Failure("Permission.Deserialize", "Module returned an unexpected response format.")]);
		}
		catch (OperationCanceledException ex)
		{
			_logger.LogWarning(ex, "Permission request to {Url} timed out.", url);
			return (default, [Error.Failure("Permission.Timeout", "The module did not respond in time.")]);
		}
		catch (HttpRequestException ex)
		{
			_logger.LogWarning(ex, "Unable to reach module permissions at {Url}", url);
			return (default, [Error.Failure("Permission.Unreachable", $"Module is unreachable: {ex.Message}")]);
		}
	}

	private async Task<(T? Value, List<Error>? Error)> HttpPostAsync<TReq, T>(string url, TReq payload, CancellationToken ct)
		where TReq : class
	{
		try
		{
			var client = _httpClientFactory.CreateClient();
			client.Timeout = TimeSpan.FromSeconds(10);

			using var response = await client.PostAsJsonAsync(url, payload, JsonOptions, ct);
			var body = await response.Content.ReadAsStringAsync(ct);

			if (!response.IsSuccessStatusCode)
			{
				var detail = SummarizeProblem(body);
				return (default, [Error.Failure("Permission.PostFailed",
					$"Module returned {(int)response.StatusCode}" + (detail is null ? "." : $": {detail}") )]);
			}

			var value = JsonSerializer.Deserialize<T>(body, JsonOptions);
			return value is null
				? (default, [Error.Failure("Permission.Deserialize", "Module returned an empty response.")])
				: (value, null);
		}
		catch (JsonException ex)
		{
			_logger.LogWarning(ex, "Failed to deserialize response from {Url}", url);
			return (default, [Error.Failure("Permission.Deserialize", "Module returned an unexpected response format.")]);
		}
		catch (OperationCanceledException ex)
		{
			_logger.LogWarning(ex, "Permission request to {Url} timed out.", url);
			return (default, [Error.Failure("Permission.Timeout", "The module did not respond in time.")]);
		}
		catch (HttpRequestException ex)
		{
			_logger.LogWarning(ex, "Unable to reach module permissions at {Url}", url);
			return (default, [Error.Failure("Permission.Unreachable", $"Module is unreachable: {ex.Message}")]);
		}
	}

	private static string? SummarizeProblem(string body)
	{
		if (string.IsNullOrWhiteSpace(body)) return null;
		try
		{
			using var doc = JsonDocument.Parse(body);
			if (doc.RootElement.TryGetProperty("detail", out var detail) && detail.ValueKind == JsonValueKind.String)
				return detail.GetString();
			if (doc.RootElement.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String)
				return title.GetString();
		}
		catch (JsonException)
		{
			// ignore — fall through to raw body
		}

		return body.Length > 200 ? body[..200] : body;
	}
}
