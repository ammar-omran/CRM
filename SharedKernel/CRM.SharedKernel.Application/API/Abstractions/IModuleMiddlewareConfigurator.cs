using Microsoft.AspNetCore.Builder;

namespace CRM.SharedKernel.Application.API.Abstractions;

public interface IModuleMiddlewareConfigurator
{
    IApplicationBuilder Configure(IApplicationBuilder app);
}
