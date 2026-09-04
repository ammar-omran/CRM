using CRM.SharedKernel.Application.API.Requests;

namespace Modules.Ticketing.Features.Tickets.Shared;

/// <summary>
/// Shared paginated + filtering request for ticket list endpoints.
/// Mirrors the old TicketFilters/CustomerTicketFilters conventions,
/// extended with category/type scoping. Bound via [FromQuery].
/// </summary>
public sealed class TicketFilterRequest : PaginationRequest
{
	/// <summary>Partial match against reference title or custom (OtherTitle).</summary>
	public string? Title { get; set; }

	/// <summary>Filter by status ids (see TicketStatusEnum). Empty = all.</summary>
	public List<int> StatusIds { get; set; } = [];

	/// <summary>Filter by severity ids. Empty = all.</summary>
	public List<int> SeverityIds { get; set; } = [];

	/// <summary>Filter by category ids. Empty = all.</summary>
	public List<int> CategoryIds { get; set; } = [];

	/// <summary>Filter by ticket-type ids. Empty = all.</summary>
	public List<int> TypeIds { get; set; } = [];

	public DateTime? FromDate { get; set; }
	public DateTime? ToDate { get; set; }
}
