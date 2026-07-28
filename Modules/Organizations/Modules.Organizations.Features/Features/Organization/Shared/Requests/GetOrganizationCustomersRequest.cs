using CRM.SharedKernel.API.Requests;

namespace Modules.Organizations.Features.Organization.Shared.Requests;

public class GetOrganizationCustomersRequest : PaginationRequest
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? OrganizationName { get; set; }
}
