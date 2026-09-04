using Modules.Ticketing.Domain.Entities;
using Modules.Ticketing.Features.Tickets.Shared;

namespace Modules.Ticketing.Features.Tickets.Shared;

internal static class TicketQueryExtensions
{
	/// <summary>
	/// Applies the shared title/status/severity/category/type/date filters.
	/// Title matches both the referenced TicketTitle name and the custom OtherTitle.
	/// </summary>
	public static IQueryable<Ticket> ApplyFilters(this IQueryable<Ticket> query, TicketFilterRequest request)
	{
		if (!string.IsNullOrWhiteSpace(request.Title))
		{
			var title = request.Title.Trim();
			query = query.Where(t =>
				(t.TicketTitle != null && t.TicketTitle.Name.Contains(title)) ||
				(t.OtherTitle != null && t.OtherTitle.Contains(title)));
		}

		if (request.StatusIds is { Count: > 0 })
		{
			var statuses = request.StatusIds
				.Where(id => Enum.IsDefined(typeof(TicketStatusEnum), id))
				.Select(id => (TicketStatusEnum)id)
				.ToList();
			if (statuses.Count > 0)
				query = query.Where(t => statuses.Contains(t.Status));
		}

		if (request.SeverityIds is { Count: > 0 })
			query = query.Where(t => t.SeverityId.HasValue && request.SeverityIds.Contains(t.SeverityId.Value));

		if (request.CategoryIds is { Count: > 0 })
			query = query.Where(t => request.CategoryIds.Contains(t.CategoryId));

		if (request.TypeIds is { Count: > 0 })
			query = query.Where(t => request.TypeIds.Contains(t.TypeId));

		if (request.FromDate.HasValue)
			query = query.Where(t => t.CreatedAt >= request.FromDate.Value);

		if (request.ToDate.HasValue)
			query = query.Where(t => t.CreatedAt <= request.ToDate.Value);

		return query;
	}

	/// <summary>
	/// Resolves the group id for the current user via the configured GroupKey claim.
	/// Mirrors the convention used at ticket creation time.
	/// Returns null when the key or claim is missing.
	/// </summary>
	public static string? ResolveGroupId(
		CRM.SharedKernel.Infrastructure.Services.CurrentUser currentUser,
		Microsoft.Extensions.Configuration.IConfiguration configuration)
	{
		var groupKey = configuration["GroupKey"];
		if (string.IsNullOrWhiteSpace(groupKey))
			return null;

		if (currentUser.TokenPayload is null)
			return null;

		if (!currentUser.TokenPayload.TryGetValue(groupKey, out var raw))
			return null;

		return raw?.ToString();
	}
}
