using Microsoft.Extensions.Logging;
using CRM.SharedKernel.Domain.Events;
using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using CRM.SharedKernel.Infrastructure.Database;
using Modules.Organizations.Domain.Entities;
using Modules.Organizations.Domain.Repositories;
using Modules.Organizations.Features.Organization.CreateOrganization.Events;
using Modules.Organizations.Features.Organization.Shared.Errors;
using Modules.Organizations.Features.Organization.Shared.Requests;
using Modules.Organizations.Features.Organization.Shared.Responses;

namespace Modules.Organizations.Features.Organization.CreateOrganization;

internal interface ICreateOrganizationHandler : IHandler
{
    Task<Result<OrganizationResponse>> HandleAsync(AddOrganizationRequest request, CancellationToken cancellationToken);
}

internal sealed class CreateOrganizationHandler(
    IRepository<Domain.Entities.Organization> orgRepo,
    IReadRepository<Domain.Entities.Agent> agentRepo,
    IReadRepository<AgentRole> agentRoleRepo,
    ICustomerRepository customerRepo,
    IReadRepository<OrganizationAgent> orgAgentsRepo,
    IEventPublisher eventPublisher,
    ILogger<CreateOrganizationHandler> logger) : ICreateOrganizationHandler
{
    public async Task<Result<OrganizationResponse>> HandleAsync(
        AddOrganizationRequest request,
        CancellationToken ct)
    {
        var errors = new List<Error>();

        var requestedAgentIds = request.Agents.Select(ra => ra.Id).ToList();
        var existingAgentIds = (await agentRepo
            .GetListAsync(
                agentRepo.Query.Where(a => requestedAgentIds.Contains(a.Id)),
                cancellation: ct))
            .Select(a => a.Id)
            .ToList();

        var missingAgentIds = requestedAgentIds.Except(existingAgentIds).ToList();
        if (missingAgentIds.Count > 0)
            errors.Add(OrganizationErrors.AgentNotFound(missingAgentIds.ToArray()));

        var requestedRoleIds = request.Agents.Select(ra => ra.Role).Distinct().ToList();
        var existingRoleIds = await agentRoleRepo
           .GetListAsync(
               query: agentRoleRepo.Query.Where(ar => requestedRoleIds.Contains(ar.Id)),
               selector: ar => ar.Id,
               cancellation: ct);

        var missingRoleIds = requestedRoleIds.Except(existingRoleIds).ToList();
        if (missingRoleIds.Count > 0)
            errors.Add(OrganizationErrors.AgentRoleNotFound(missingRoleIds.ToArray()));

        var nameExists = await orgRepo.AnyAsync(o => o.Name == request.Name, ct);
        if (nameExists)
            errors.Add(OrganizationErrors.NameAlreadyExists(request.Name));

        if (errors.Count > 0)
        {
            errors.ForEach(e => logger.LogInformation(
                "ErrorCode: '{Code}' - Description: {Description}", e.Code, e.Description));
            return errors;
        }

        var organization = new Domain.Entities.Organization
        {
            Name = request.Name,
            CreatedAt = DateTime.UtcNow,
            OrganizationAgents = request.Agents.Select(
                a => new OrganizationAgent
                {
                    AgentId = a.Id,
                    AgentRoleId = a.Role,
                }).ToList(),
            OrganizationCustomers = [],
        };

        var customersIdsToAssign = request.Customers.Select(x => x.Id);
        var existedCustomers = (await customerRepo.GetListAsync(
            query: customerRepo.Query.Where(c => customersIdsToAssign.Contains(c.ReferenceId)),
            cancellation: ct
        )).Distinct().ToDictionary(x => x.ReferenceId, x => x);

        foreach (var customerId in customersIdsToAssign)
        {
            if (existedCustomers.TryGetValue(customerId, out var value))
            {
                organization.OrganizationCustomers.Add(new OrganizationCustomer { Customer = value });
            }
            else
            {
                try
                {
                    var fetchedCustomer = await customerRepo.FetchCustomerAsync(customerId);
                    organization.OrganizationCustomers.Add(new OrganizationCustomer { Customer = fetchedCustomer });
                }
                catch (KeyNotFoundException ex)
                {
                    logger.LogInformation(ex, ex.Message);
                    return OrganizationErrors.CustomerNotFound(ex.Message);
                }
                catch (ApplicationException ex)
                {
                    logger.LogInformation(ex, ex.Message);
                    return OrganizationErrors.FailedToFetchCustomer(ex.Message);
                }
            }
        }

        try
        {
            await orgRepo.Add(organization, ct);

            logger.LogInformation("Organization Created with ID {OrganizationId}", organization.Id);

            organization.OrganizationAgents = await orgAgentsRepo.GetListAsync(
                orgAgentsRepo.Query.Where(orgAgent => orgAgent.OrganizationId == organization.Id),
                includes: [nameof(OrganizationAgent.Agent), nameof(OrganizationAgent.AgentRole)],
                cancellation: ct);

            await eventPublisher.PublishAsync(new OrganizationCreatedEvent(organization.Id, organization.Name), ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save Organization name: {Name}", organization.Name);
            return OrganizationErrors.AddingOrganizationFailed(organization.Name);
        }

        return new OrganizationResponse
        {
            Id = organization.Id,
            Name = organization.Name,
            CreatedAt = organization.CreatedAt,
            AgentsCount = organization.OrganizationAgents.Count,
            CustomersCount = organization.OrganizationCustomers.Count,
        };
    }
}
