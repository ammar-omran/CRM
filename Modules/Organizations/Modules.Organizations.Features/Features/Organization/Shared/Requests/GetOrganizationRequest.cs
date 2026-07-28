using CRM.SharedKernel.API.Requests;

namespace Modules.Organizations.Features.Organization.Shared.Requests;

public class GetOrganizationRequest : PaginationRequest
{
    public string? Name { get; set; }
}
