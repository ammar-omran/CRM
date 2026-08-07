using CRM.SharedKernel.Domain.Interfaces;
using Modules.Users.Domain.OrganizationAggregate;

namespace Modules.Users.Domain.Repositories;

public interface ICustomerRepository : IRepository<Customer>
{
	Task<Customer> FetchCustomerAsync(int referenceId);
}
