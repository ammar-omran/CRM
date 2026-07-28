using Microsoft.AspNetCore.Builder;

namespace CRM.SharedKernel.API.Abstractions;

public interface IModuleMiddlewareConfigurator
{
    IApplicationBuilder Configure(IApplicationBuilder app);
}
