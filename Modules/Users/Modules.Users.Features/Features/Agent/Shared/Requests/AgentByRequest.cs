using CRM.SharedKernel.Application.API.Requests;

namespace Modules.Users.Features.Agent.Shared.Requests;

public class AgentByRequest : PaginationRequest
{
    public string? Email { get; set; }
}
