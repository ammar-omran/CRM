using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using Modules.Customers.Features.Customers.Shared;
using Modules.Customers.Infrastructure.Database;

namespace Modules.Customers.Features.Customers.GetAllCustomers;

public sealed record GetAllCustomersRequest(int Skip, int Limit);

internal interface IGetAllCustomersHandler : IHandler
{
    Task<Result<IReadOnlyList<CustomerResponse>>> HandleAsync(GetAllCustomersRequest request, CancellationToken cancellationToken);
}

internal sealed class GetAllCustomersHandler(
    CustomersDbContext context,
    ILogger<GetAllCustomersHandler> logger)
    : IGetAllCustomersHandler
{
    public async Task<Result<IReadOnlyList<CustomerResponse>>> HandleAsync(
        GetAllCustomersRequest request,
        CancellationToken cancellationToken)
    {
        var customers = await context.Customers
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedDate)
            .Skip(request.Skip)
            .Take(request.Limit)
            .Select(c => new CustomerResponse(
                c.Id,
                c.Name,
                c.Email,
                c.PhoneNumber.Number,
                c.PhoneNumber.CountryCode,
                c.IsEmailVerified,
                c.CreatedDate))
            .ToListAsync(cancellationToken);

        logger.LogInformation("Retrieved {Count} customers", customers.Count);
        return customers;
    }
}
