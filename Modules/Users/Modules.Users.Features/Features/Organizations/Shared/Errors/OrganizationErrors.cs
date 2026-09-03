using CRM.SharedKernel.Domain.Results;
using Modules.Users.Features.Organizations.Shared.Responses;

namespace Modules.Users.Features.Organizations.Shared.Errors;

internal static class OrganizationErrors
{
    private const string Prefix = "Organization";

    internal static Result<OrganizationResponse> AddingOrganizationFailed(string name)
        => Error.Unexpected($"{Prefix}.{nameof(AddingOrganizationFailed)}", $"An error occurred while creating an Organization with Name {name}");

    internal static Error AgentNotFound(int[] agentIds)
        => Error.Validation($"{Prefix}.{nameof(AgentNotFound)}", $"The following Agents were not found: {string.Join(", ", agentIds)}");

    internal static Error CustomerNotFound(string description)
        => Error.NotFound($"{Prefix}.{nameof(CustomerNotFound)}", description);

    internal static Error FailedToFetchCustomer(string description)
        => Error.Failure($"{Prefix}.{nameof(FailedToFetchCustomer)}", description);

    internal static Error AgentRoleNotFound(string[] agentRoleIds)
        => Error.Validation($"{Prefix}.{nameof(AgentRoleNotFound)}", $"The following Agent Roles were not found: {string.Join(", ", agentRoleIds)}");

    internal static Error NameAlreadyExists(string name)
        => Error.Conflict($"{Prefix}.{nameof(NameAlreadyExists)}", $"An Organization name already exist '{name}'");
}
