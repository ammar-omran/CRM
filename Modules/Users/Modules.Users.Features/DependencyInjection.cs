using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using CRM.SharedKernel.Application.API.Abstractions;
using CRM.SharedKernel.Application.Extensions;
using Modules.Users.Features.Middlewares;
using CRM.SharedKernel.Domain.Events;
using CRM.SharedKernel.Application;
using CRM.SharedKernel.Application.API.Extensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class UsersModuleRegistration
{
	public static IServiceCollection AddUsersModule(this IServiceCollection services, IConfiguration configuration)
	{
		return services
			.AddUsersInfrastructure(configuration)
			.AddUsersModuleApi(configuration);
	}

	private static IServiceCollection AddUsersModuleApi(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddScoped<IEventPublisher, EventPublisher>();
		services.AddEmailSender(configuration);
		services.RegisterApiEndpointsFromAssemblyContaining(typeof(UsersModuleRegistration));
		services.AddSharedKernelModuleMiddlewares();
		services.RegisterModuleMiddlewaresFromAssemblyContaining(typeof(UsersModuleRegistration));
		services.RegisterHandlersFromAssemblyContaining(typeof(UsersModuleRegistration));
		services.AddValidatorsFromAssembly(typeof(UsersModuleRegistration).Assembly);

		return services;
	}
}

public class StocksMiddlewareConfigurator : IModuleMiddlewareConfigurator
{
	public IApplicationBuilder Configure(IApplicationBuilder app)
	{
		return app.UseMiddleware<CheckRevocatedTokensMiddleware>();
	}
}
