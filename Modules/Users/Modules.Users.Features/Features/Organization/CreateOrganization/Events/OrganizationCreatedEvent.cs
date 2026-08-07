using CRM.SharedKernel.Domain.Events;

namespace Modules.Users.Features.Organization.CreateOrganization.Events;

public sealed record OrganizationCreatedEvent(int OrganizationId, string OrganizationName) : IEvent;
