using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using CRM.SharedKernel.API.Abstractions;

namespace CRM.SharedKernel.API.Extensions;

public static class MiddlewareRegistrationExtensions
{
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
