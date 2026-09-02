using CRM.SharedKernel.Domain.Results;

namespace Modules.Users.Features.Agent.Shared.Errors;

internal static class AgentErrors
{
    private const string Prefix = "Agent";

    internal static Error AddingAgentFailed(string userId)
        => Error.Unexpected($"{Prefix}.{nameof(AddingAgentFailed)}", $"An error occurred while adding an Agent for User {userId}");

    internal static Error EmailAlreadyExists(string email)
        => Error.Conflict($"{Prefix}.{nameof(EmailAlreadyExists)}", $"An agent with the same email '{email}' already exists.");

    internal static Error UserAlreadyAgent(string userId)
        => Error.Conflict($"{Prefix}.{nameof(UserAlreadyAgent)}", $"An agent for user '{userId}' already exists.");

    internal static Error UserNotFound(string userId)
        => Error.NotFound($"{Prefix}.{nameof(UserNotFound)}", $"User '{userId}' was not found.");
}
