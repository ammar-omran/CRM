using System.Diagnostics;
using CRM.SharedKernel.Application.API.Abstractions;
using CRM.SharedKernel.Domain.Modules;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Builder;

namespace CRM.SharedKernel.Application.API.Tracing;

public sealed class TracingMiddleware : IDisposable
{
	private readonly RequestDelegate _next;
	private readonly ActivitySource _activitySource;
	private readonly IModuleManifest _manifest;

	public TracingMiddleware(RequestDelegate next, IModuleManifest manifest)
	{
		_next = next;
		_manifest = manifest;
		_activitySource = new ActivitySource(_manifest.Identity.ModuleId);
	}

	public async Task InvokeAsync(HttpContext context)
	{
		ArgumentNullException.ThrowIfNull(context);

		if (!MatchesPathPrefix(context.Request.Path))
		{
			await _next(context);
			return;
		}

		var operationName = GetOperationName(context);
		using var activity = _activitySource.StartActivity($"{_manifest.Identity.ModuleId}.{operationName}");

		activity?.SetTag("module", _manifest.Identity.ModuleId);
		activity?.SetTag("http.method", context.Request.Method);
		activity?.SetTag("http.path", context.Request.Path);
		activity?.SetTag("operation", operationName);

		try
		{
			await _next(context);

			activity?.SetTag("http.status_code", context.Response.StatusCode);
			activity?.SetStatus(context.Response.StatusCode >= 400
				? ActivityStatusCode.Error
				: ActivityStatusCode.Ok);
		}
		catch (Exception ex)
		{
			activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
			activity?.SetTag("error.message", ex.Message);
			throw;
		}
	}

	private bool MatchesPathPrefix(PathString path)
	{
		foreach (var prefix in _manifest.Api?.RoutePrefixs ?? [])
		{
			if (path.StartsWithSegments(prefix, StringComparison.Ordinal))
				return true;
		}
		return false;
	}

	private static string GetOperationName(HttpContext context)
	{
		var method = context.Request.Method.ToUpperInvariant();
		return method switch
		{
			"POST" => "create",
			"GET" => "get",
			"PUT" or "PATCH" => "update",
			"DELETE" => "delete",
			_ => "unknown"
		};
	}

	public void Dispose()
	{
		_activitySource.Dispose();
	}
}

public sealed class TracingMiddlewareConfigurator : IModuleMiddlewareConfigurator
{
	public IApplicationBuilder Configure(IApplicationBuilder app)
		=> app.UseMiddleware<TracingMiddleware>();
}
