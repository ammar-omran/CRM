using System.Diagnostics;
using CRM.SharedKernel.Application.API.Abstractions;
using CRM.SharedKernel.Domain.Modules;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace CRM.SharedKernel.Application.API.Tracing;

public sealed class TracingMiddleware : IDisposable
{
	private const string InternalPrefix = "/internal";

	private readonly RequestDelegate _next;
    private readonly ActivitySource _activitySource;
    private readonly IModuleManifest _manifest;

    public TracingMiddleware(
        RequestDelegate next,
        IModuleManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(manifest);

        _next = next;
        _manifest = manifest;

        _activitySource = new ActivitySource(
            _manifest.Identity.ModuleId);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!MatchesPathPrefix(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var endpoint = context.GetEndpoint();
        var route = GetRoute(endpoint);

        var operationName = GetOperationName(
            context,
            route);

        using var activity = _activitySource.StartActivity(
            $"{_manifest.Identity.ModuleId}.{operationName}",
            ActivityKind.Internal);

        if (activity is not null)
        {
            // Module
            activity.SetTag(
                "module.id",
                _manifest.Identity.ModuleId);

            // Operation
            activity.SetTag(
                "module.operation",
                operationName);

            // HTTP
            activity.SetTag(
                "http.request.method",
                context.Request.Method);

            if (!string.IsNullOrWhiteSpace(route))
            {
                activity.SetTag(
                    "http.route",
                    route);
            }
        }

        try
        {
            await _next(context);

            if (activity is null)
                return;

            var statusCode = context.Response.StatusCode;

            activity.SetTag(
                "http.response.status_code",
                statusCode);

            activity.SetTag(
                "module.outcome",
                GetOutcome(statusCode));

            if (statusCode >= StatusCodes.Status500InternalServerError)
            {
                activity.SetStatus(ActivityStatusCode.Error);
            }
        }
        catch (Exception ex)
        {
            if (activity is not null)
            {
                activity.SetStatus(
                    ActivityStatusCode.Error);

                activity.SetTag(
                    "module.outcome",
                    "exception");

                activity.AddException(ex);
            }

            throw;
        }
    }

	private bool MatchesPathPrefix(PathString path)
	{
		if (path.StartsWithSegments(
				InternalPrefix,
				StringComparison.Ordinal))
		{
			return true;
		}

		foreach (var prefix in _manifest.Api?.RoutePrefixs ?? [])
        {
            if (path.StartsWithSegments(
                    prefix,
                    StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static string GetOperationName(
        HttpContext context,
        string? route)
    {
        var method = context.Request.Method.ToLowerInvariant();

        var lastSegment = GetLastRouteSegment(route);

        return string.IsNullOrWhiteSpace(lastSegment)
            ? method
            : $"{lastSegment}.{method}";
    }

    private static string? GetLastRouteSegment(string? route)
    {
        if (string.IsNullOrWhiteSpace(route))
            return null;

        var segments = route
            .Split(
                '/',
                StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length == 0)
            return null;

        var lastSegment = segments[^1];

        // Normalize route parameters:
        // /tickets/{id} -> id
        if (lastSegment.StartsWith('{') &&
            lastSegment.EndsWith('}'))
        {
            lastSegment = lastSegment[1..^1];

            // Handle constraints such as {id:int}
            var constraintIndex = lastSegment.IndexOf(':');

            if (constraintIndex >= 0)
            {
                lastSegment = lastSegment[..constraintIndex];
            }
        }

        return NormalizeSegment(lastSegment);
    }

    private static string NormalizeSegment(string value)
    {
        return value
            .Replace("-", "_")
            .Replace(".", "_")
            .ToLowerInvariant();
    }

    private static string? GetRoute(Endpoint? endpoint)
    {
        return endpoint switch
        {
            RouteEndpoint routeEndpoint
                => routeEndpoint.RoutePattern.RawText,

            _ => null
        };
    }

    private static string GetOutcome(int statusCode)
    {
        return statusCode switch
        {
            >= 500 => "server_error",
            >= 400 => "client_error",
            >= 300 => "redirect",
            >= 200 => "success",
            _ => "other"
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
