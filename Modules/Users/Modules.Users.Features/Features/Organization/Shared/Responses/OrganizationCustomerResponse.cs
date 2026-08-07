namespace Modules.Users.Features.Organization.Shared.Responses;

public class OrganizationCustomerResponse
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
}
