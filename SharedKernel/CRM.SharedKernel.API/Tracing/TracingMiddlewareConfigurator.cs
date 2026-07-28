using Microsoft.AspNetCore.Builder;
using CRM.SharedKernel.API.Abstractions;

namespace CRM.SharedKernel.API.Tracing;

public sealed class TracingMiddlewareConfigurator : IModuleMiddlewareConfigurator
{
    public IApplicationBuilder Configure(IApplicationBuilder app)
        => app.UseMiddleware<TracingMiddleware>();
}
