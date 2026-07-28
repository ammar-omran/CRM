using CRM.SharedKernel.API.Requests;

namespace Modules.Organizations.Features.Agent.Shared.Requests;

public class AgentByRequest : PaginationRequest
{
    public string? Email { get; set; }
}
