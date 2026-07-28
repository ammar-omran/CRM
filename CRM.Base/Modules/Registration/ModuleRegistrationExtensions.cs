using CRM.Base.Modules.DependencyResolution;
using CRM.Base.Modules.Persistence;
using CRM.Base.Modules.Registry;
using CRM.Base.Modules.Registration;
using CRM.Base.Modules.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CRM.SharedKernel.Application.Extensions;
using CRM.SharedKernel.Application;
using CRM.SharedKernel.Domain.Events;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Registers the module registration pipeline into the platform's dependency injection container.
/// </summary>
public static class ModuleRegistrationExtensions
{
    /// <summary>
    /// Adds the complete module registration pipeline to the service collection.
    /// Includes: SQLite persistence, runtime catalog, validators, dependency resolver, and registrar.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="platformVersion">The current platform version for compatibility checks.</param>
    /// <param name="sharedKernelVersion">The current SharedKernel version for compatibility checks.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddModuleRegistration(
        this IServiceCollection services,
        string platformVersion,
        string sharedKernelVersion)
    {
        // SQLite persistence
        services.AddDbContextFactory<PlatformDbContext>(options =>
            options.UseSqlite("Data Source=platform.db"));

        services.AddScoped<IEventPublisher, EventPublisher>();
        services.RegisterHandlersFromAssemblyContaining(typeof(ModuleRegistrationExtensions));

        services.AddSingleton<IModulePersistence, ModulePersistence>();

        // HTTP client for fetching manifests
        services.AddHttpClient();

        // Runtime catalog (startup loader + in-memory cache)
        services.AddSingleton<ModuleCatalog>();

        // In-memory registry (used by validators and registrar)
        services.AddSingleton<IModuleRegistry, ModuleRegistry>();

        // Dependency resolver
        services.AddSingleton<IDependencyResolver, DependencyResolver>();

        // Validators
        services.AddSingleton<IModuleValidator, ManifestValidator>();

        services.AddSingleton<IModuleValidator>(sp =>
        {
            var registry = sp.GetRequiredService<IModuleRegistry>();
            return new CompatibilityValidator(platformVersion, sharedKernelVersion);
        });

        services.AddSingleton<IModuleValidator>(sp =>
        {
            var registry = sp.GetRequiredService<IModuleRegistry>();
            return new DependencyValidator(() => registry.GetAll().Select(m => m.ModuleId).ToList());
        });

        services.AddSingleton<IModuleValidator>(sp =>
        {
            var registry = sp.GetRequiredService<IModuleRegistry>();
            return new CapabilityValidator(() =>
                registry.GetAll().Select(m => m.Manifest).ToList());
        });

        services.AddSingleton<IModuleValidator>(sp =>
        {
            var registry = sp.GetRequiredService<IModuleRegistry>();
            return new RouteValidator(() =>
                registry.GetAll().Select(m => m.Manifest).ToList());
        });

        // Validator pipeline
        services.AddSingleton<ValidatorPipeline>();


        // Registrar
        services.AddScoped<IModuleRegistrar, ModuleRegistrar>();

        return services;
    }

    /// <summary>
    /// Ensures the SQLite database is created and migrations are applied.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application for chaining.</returns>
    public static async Task<WebApplication> InitializePlatformDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<PlatformDbContext>>();
        await using var db = await factory.CreateDbContextAsync();
        await db.Database.EnsureCreatedAsync();

        return app;
    }

    /// <summary>
    /// Loads all registered modules from the database into the runtime catalog.
    /// If a module is offline, the cached manifest is used and the module is marked unavailable.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application for chaining.</returns>
    public static async Task<WebApplication> LoadModuleCatalogAsync(this WebApplication app)
    {
        var catalog = app.Services.GetRequiredService<ModuleCatalog>();
        await catalog.LoadAsync();
        return app;
    }
}
