using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using Modules.Customers.Domain.Errors;
using Modules.Customers.Features.Customers.Shared;
using Modules.Customers.Infrastructure.Database;

namespace Modules.Customers.Features.Customers.GetCustomerById;

internal interface IGetCustomerByIdHandler : IHandler
{
    Task<Result<CustomerResponse>> HandleAsync(int customerId, CancellationToken cancellationToken);
}

internal sealed class GetCustomerByIdHandler(
    CustomersDbContext context,
    ILogger<GetCustomerByIdHandler> logger)
    : IGetCustomerByIdHandler
{
    public async Task<Result<CustomerResponse>> HandleAsync(
        int customerId,
        CancellationToken cancellationToken)
    {
        var customer = await context.Customers
            .FirstOrDefaultAsync(c => c.Id == customerId, cancellationToken);

        if (customer is null)
        {
            logger.LogInformation("Customer with ID {CustomerId} not found", customerId);
            return CustomerErrors.NotFound(customerId);
        }

        logger.LogInformation("Retrieved customer with ID: {CustomerId}", customerId);
        return new CustomerResponse(
            customer.Id,
            customer.Name,
            customer.Email,
            customer.PhoneNumber.Number,
            customer.PhoneNumber.CountryCode,
            customer.IsEmailVerified,
            customer.CreatedDate);
    }
}
