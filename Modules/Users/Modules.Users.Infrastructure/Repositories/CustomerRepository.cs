using System.Net.Http.Json;
using CRM.SharedKernel.Infrastructure.Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Modules.Users.Domain.OrganizationAggregate;
using Modules.Users.Domain.Repositories;
using Modules.Users.Infrastructure.Database;

namespace Modules.Users.Infrastructure.Repositories;

public class CustomerRepository : EfRepository<Customer>, ICustomerRepository
{
	private readonly OrganizationsDbContext _context;
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly ILogger<CustomerRepository> _logger;

	public CustomerRepository(
		OrganizationsDbContext dbContext,
		IHttpClientFactory httpClientFactory,
		ILogger<CustomerRepository> logger)
		: base(dbContext)
	{
		_context = dbContext;
		_httpClientFactory = httpClientFactory;
		_logger = logger;
	}

	public async Task<Customer> FetchCustomerAsync(int referenceId)
	{
		_logger.LogInformation(
			"Fetching customer with ReferenceId {ReferenceId} from Modules.Customers API",
			referenceId);

		var client = _httpClientFactory.CreateClient("Modules.CustomersApi");

		HttpResponseMessage response;
		try
		{
			response = await client.GetAsync($"api/Customer/{referenceId}");
		}
		catch (HttpRequestException ex)
		{
			_logger.LogError(ex,
				"HTTP request to Modules.Customers failed for ReferenceId {ReferenceId}",
				referenceId);
			throw new ApplicationException(
				$"Unable to reach Modules.Customers service while fetching customer {referenceId}.", ex);
		}

		if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
			throw new KeyNotFoundException($"Customer with ReferenceId {referenceId} was not found in Modules.Customers.");

		if (!response.IsSuccessStatusCode)
			throw new ApplicationException(
				$"Modules.Customers returned {(int)response.StatusCode} while fetching customer {referenceId}.");

		var dto = await response.Content.ReadFromJsonAsync<CustomersDto>();
		if (dto is null)
			throw new ApplicationException(
				$"Modules.Customers returned an empty body for customer {referenceId}.");

		_logger.LogInformation(
			"Successfully fetched customer ReferenceId {ReferenceId} - Name: {Name}",
			referenceId, dto.Name);

		return new Customer
		{
			ReferenceId = referenceId,
			Name = dto.Name ?? string.Empty,
			Email = dto.Email ?? string.Empty,
			Phone = dto.Phone ?? string.Empty,
			CreatedAt = DateTime.UtcNow,
		};
	}

	private sealed record CustomersDto(
		int Id,
		string? Name,
		string? Email,
		string? Phone);
}
