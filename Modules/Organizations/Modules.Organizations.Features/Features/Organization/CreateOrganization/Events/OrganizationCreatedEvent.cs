using CRM.SharedKernel.Domain.Events;

namespace Modules.Organizations.Features.Organization.CreateOrganization.Events;

public sealed record OrganizationCreatedEvent(int OrganizationId, string OrganizationName) : IEvent;
