using Microsoft.AspNetCore.Builder;

namespace CRM.SharedKernel.API.Abstractions;

/// <summary>
/// Represents an interface defining the contract for mapping SharedKernel.API.endpoints into a web application.
/// </summary>
public interface IApiEndpoint
{
    /// <summary>
    /// Maps the SharedKernel.API.endpoint for the specified web application instance.
    /// </summary>
    /// <param name="app">The web application instance where the SharedKernel.API.endpoint will be mapped.</param>
    void MapEndpoint(WebApplication app);
}
