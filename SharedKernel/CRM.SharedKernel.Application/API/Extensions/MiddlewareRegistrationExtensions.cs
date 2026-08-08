using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CRM.SharedKernel.Application.API.Abstractions;
using CRM.SharedKernel.Application.API.Tracing;

namespace CRM.SharedKernel.Application.API.Extensions;

public static class MiddlewareRegistrationExtensions
{

	/// <summary>
	/// Registers all <see cref="IModuleMiddlewareConfigurator"/> implementations from the assembly containing the specified type <typeparamref name="T"/> into the service collection.
	/// </summary>
	/// <param name="marker">A type whose assembly will be scanned for <see cref="IModuleMiddlewareConfigurator"/> implementations.</param>
	/// <param name="services">The service collection to register the middlewares into.</param>
	/// <returns>The modified service collection with registered middlewares.</returns>
	public static IServiceCollection RegisterModuleMiddlewaresFromAssemblyContaining(this IServiceCollection services, Type marker)
	{
		var assembly = marker.Assembly;

		var middlewareTypes = assembly.GetTypes()
			.Where(t => t.IsAssignableTo(typeof(IModuleMiddlewareConfigurator)) && t is { IsClass: true, IsAbstract: false, IsInterface: false });

		var serviceDescriptors = middlewareTypes
			.Select(type => ServiceDescriptor.Singleton(typeof(IModuleMiddlewareConfigurator), type))
			.ToArray();

		services.TryAddEnumerable(serviceDescriptors);
		return services;
	}

	public static IServiceCollection AddSharedKernelModuleMiddlewares(this IServiceCollection services)
	{
		services.TryAddEnumerable(ServiceDescriptor.Singleton(
			typeof(IModuleMiddlewareConfigurator), typeof(TracingMiddlewareConfigurator)));
		return services;
	}

	public static IApplicationBuilder UseModuleMiddlewares(this IApplicationBuilder app)
	{
		var configurators = app.ApplicationServices.GetServices<IModuleMiddlewareConfigurator>();

		foreach (var configurator in configurators)
		{
			configurator.Configure(app);
		}

		return app;
	}
}
