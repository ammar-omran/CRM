namespace Modules.Users.Features.Organization.Shared.Responses;

public class OrganizationResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int AgentsCount { get; set; }
    public int CustomersCount { get; set; }
}
