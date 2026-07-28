using CRM.SharedKernel.API.Abstractions;
using CRM.SharedKernel.API.Tracing;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class TracingExtensions
{
    public static IServiceCollection AddModuleTracing(
        this IServiceCollection services,
        string moduleName,
        params string[] pathPrefixes)
    {
        services.Configure<TracingOptions>(opt =>
        {
            opt.ModuleName = moduleName;
            opt.PathPrefixes = pathPrefixes;
        });

        services.AddSingleton<IModuleMiddlewareConfigurator, TracingMiddlewareConfigurator>();

        return services;
    }
}
