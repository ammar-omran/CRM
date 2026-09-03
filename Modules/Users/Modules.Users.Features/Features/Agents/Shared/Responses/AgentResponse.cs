namespace Modules.Users.Features.Agents.Shared.Responses;

public record AgentResponse(
    int Id,
    string UserId,
    string Name,
    string Email
);
