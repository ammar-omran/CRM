using CRM.SharedKernel.Domain.Results;

namespace Modules.Users.Features.Agent.Shared.Errors;

internal static class AgentErrors
{
    private const string Prefix = "Agent";

    internal static Error AddingAgentFailed(string email)
        => Error.Unexpected($"{Prefix}.{nameof(AddingAgentFailed)}", $"An error occurred while adding an Agent with Email {email}");

    internal static Error EmailAlreadyExists(string email)
        => Error.Conflict($"{Prefix}.{nameof(EmailAlreadyExists)}", $"An agent with the same email '{email}' already exists.");
}
