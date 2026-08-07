namespace Modules.Users.Features.Agent.Shared.Responses;

public record AgentResponse(
    int Id,
    string Name,
    string Email,
    string Phone
);
