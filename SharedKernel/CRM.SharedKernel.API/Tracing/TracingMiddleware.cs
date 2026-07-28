using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace CRM.SharedKernel.API.Tracing;

public sealed class TracingMiddleware : IDisposable
{
    private readonly RequestDelegate _next;
    private readonly TracingOptions _options;
    private readonly ActivitySource _activitySource;

    public TracingMiddleware(RequestDelegate next, IOptions<TracingOptions> options)
    {
        _next = next;
        _options = options.Value;
        _activitySource = new ActivitySource(_options.ModuleName);
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
        using var activity = _activitySource.StartActivity($"{_options.ModuleName}.{operationName}");

        activity?.SetTag("module", _options.ModuleName);
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
        foreach (var prefix in _options.PathPrefixes)
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
