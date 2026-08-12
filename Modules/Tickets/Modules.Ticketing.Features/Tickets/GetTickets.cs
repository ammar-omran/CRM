using CRM.SharedKernel.Domain.Handlers;
using CRM.SharedKernel.Domain.Results;
using Modules.Ticketing.Infrastructure.Database;
using Modules.Ticketing.Features.Tickets;
using Microsoft.EntityFrameworkCore;

namespace Modules.Ticketing.Features.Tickets;

public interface IGetTicketsHandler : IHandler
{
	Task<Result<IReadOnlyList<TicketResponse>>> HandleAsync(CancellationToken cancellationToken = default);
}

internal sealed class GetTicketsHandler : IGetTicketsHandler
{
	private readonly TicketingDbContext _dbContext;

	public GetTicketsHandler(TicketingDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task<Result<IReadOnlyList<TicketResponse>>> HandleAsync(CancellationToken cancellationToken = default)
	{
		var tickets = await _dbContext.Tickets
			.AsNoTracking()
			.OrderByDescending(t => t.CreatedAt)
			.Select(t => new
			{
				t.Id,
				t.OtherTitle,
				ReferenceTitle = t.TicketTitle!.Name,
				t.Description,
				t.Status,
				t.CreatedAt,
				t.UpdatedAt
			})
			.ToListAsync(cancellationToken);

		return tickets
			.Select(t => new TicketResponse(
				t.Id,
				t.ReferenceTitle ?? t.OtherTitle,
				t.Description,
				t.Status.ToString(),
				t.CreatedAt,
				t.UpdatedAt))
			.ToList();
	}
}
