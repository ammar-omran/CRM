using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using CRM.SharedKernel.API.Abstractions;
using CRM.SharedKernel.Application.Extensions;
using Modules.Users.Features.Middlewares;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class UsersModuleRegistration
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddUsersModuleApi()
            .AddUsersInfrastructure(configuration);
    }
    
    private static IServiceCollection AddUsersModuleApi(this IServiceCollection services)
    {
        services.RegisterApiEndpointsFromAssemblyContaining(typeof(UsersModuleRegistration));
        
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
