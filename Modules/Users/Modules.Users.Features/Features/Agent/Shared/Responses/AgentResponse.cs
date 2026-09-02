namespace Modules.Users.Features.Agent.Shared.Responses;

public record AgentResponse(
    int Id,
    string UserId,
    string Name,
    string Email
);
