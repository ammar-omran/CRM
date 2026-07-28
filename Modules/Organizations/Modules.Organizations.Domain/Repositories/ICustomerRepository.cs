using CRM.SharedKernel.Infrastructure.Database;
using Modules.Organizations.Domain.Entities;

namespace Modules.Organizations.Domain.Repositories;

public interface ICustomerRepository : IRepository<Customer>
{
	Task<Customer> FetchCustomerAsync(int referenceId);
}
