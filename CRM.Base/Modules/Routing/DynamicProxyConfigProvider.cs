using System.Threading;
using CRM.Base.Modules.Persistence;
using Microsoft.Extensions.Logging;
using Yarp.ReverseProxy.Configuration;

namespace CRM.Base.Modules.Routing;

/// <summary>
/// Generates YARP reverse proxy configuration dynamically from the module catalog.
/// Rebuilds the entire configuration snapshot when modules are registered or removed.
/// </summary>
public sealed class DynamicProxyConfigProvider : IProxyConfigProvider
{
    private readonly ModuleCatalog _catalog;
    private readonly ILogger<DynamicProxyConfigProvider> _logger;
    private volatile int _revisionId;
    private CancellationTokenSource _cts = new();

    /// <summary>
    /// Initializes the provider with the module catalog.
    /// </summary>
    public DynamicProxyConfigProvider(ModuleCatalog catalog, ILogger<DynamicProxyConfigProvider> logger)
    {
        _catalog = catalog;
        _logger = logger;

        _catalog.OnChanged = Reload;
    }

    /// <inheritdoc />
    public IProxyConfig GetConfig() => new DynamicProxyConfig(
        Interlocked.Increment(ref _revisionId).ToString(),
        BuildRoutes(),
        BuildClusters(),
        _cts);

    /// <summary>
    /// Signals YARP to reload configuration from <see cref="GetConfig"/>.
    /// Called automatically when the module catalog changes.
    /// </summary>
    public void Reload()
    {
        var oldCts = Interlocked.Exchange(ref _cts, new CancellationTokenSource());
        oldCts.Cancel();
        oldCts.Dispose();

        _logger.LogDebug("Proxy configuration reloaded (revision {Revision})", _revisionId);
    }

    private List<RouteConfig> BuildRoutes()
    {
        var routes = new List<RouteConfig>();
        var order = 0;

        foreach (var entry in _catalog.GetAll())
        {
            if (entry.Manifest?.Api is not { } api)
            {
                continue;
            }

            var moduleId = entry.Manifest.Identity.ModuleId;
            var clusterId = ToClusterId(moduleId);

            // Specific routes for anonymous paths (no auth required)
            if (api.AnonymousPaths is { Count: > 0 })
            {
                foreach (var path in api.AnonymousPaths)
                {
                    routes.Add(new RouteConfig
                    {
                        RouteId = $"{moduleId}:anon:{NormalizeRouteId(path)}",
                        Order = order++,
                        ClusterId = clusterId,
                        Match = new RouteMatch
                        {
                            Path = EnsureCatchAll(path)
                        }
                    });
                }
            }

            // Catch-all route for authenticated traffic
            routes.Add(new RouteConfig
            {
                RouteId = $"{moduleId}:all",
                Order = order++,
                ClusterId = clusterId,
                AuthorizationPolicy = "Authenticated",
                Match = new RouteMatch
                {
                    Path = $"{api.RoutePrefix}/{{**catch-all}}"
                }
            });
        }

        return routes;
    }

    private List<ClusterConfig> BuildClusters()
    {
        var clusters = new List<ClusterConfig>();

        foreach (var entry in _catalog.GetAll())
        {
            if (entry.Manifest is null || !entry.IsAvailable)
            {
                continue;
            }

            var moduleId = entry.Manifest.Identity.ModuleId;
            var clusterId = ToClusterId(moduleId);

            // Configure active health checks if the module exposes a health endpoint
            bool considerHealthCheck = false;
            var healthCheck = entry.Manifest.Health is { } health && considerHealthCheck
                ? new HealthCheckConfig
                {
                    Active = new ActiveHealthCheckConfig
                    {
                        Enabled = true,
                        Path = health.Endpoint,
                        Interval = TimeSpan.FromSeconds(30),
                        Timeout = TimeSpan.FromSeconds(5)
                    }
                }
                : null;

            clusters.Add(new ClusterConfig
            {
                ClusterId = clusterId,
                HealthCheck = healthCheck,
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    ["primary"] = new DestinationConfig
                    {
                        Address = entry.Entity.BaseUrl
                    }
                }
            });
        }

        return clusters;
    }

    /// <summary>
    /// Converts a module ID to a valid YARP cluster ID.
    /// Example: "crm.users" → "crm-users".
    /// </summary>
    private static string ToClusterId(string moduleId) =>
        moduleId.Replace('.', '-');

    /// <summary>
    /// Normalizes a route path to a valid route ID segment.
    /// Example: "/api/users/login" → "api-users-login".
    /// </summary>
    private static string NormalizeRouteId(string path) =>
        path.Trim('/').Replace('/', '-').Replace('{', '_').Replace('}', '_');

    /// <summary>
    /// Ensures the path ends with a catch-all suffix if it doesn't already.
    /// </summary>
    private static string EnsureCatchAll(string path)
    {
        path = path.TrimEnd('/');
        if (!path.Contains("{**"))
        {
            path = $"{path}/{{**catch-all}}";
        }

        return path;
    }
}
