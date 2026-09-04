using System.Security.Claims;
using CRM.SharedKernel.Domain.Authorization;
using Microsoft.EntityFrameworkCore;
using Modules.Ticketing.Domain.Policies;
using Modules.Ticketing.Infrastructure.Database;

namespace Modules.Ticketing.Features.Tickets.Shared;

/// <summary>
/// Data-level access checks for single-ticket reads.
/// The <c>view-any</c> policy lets holders of any view permission reach the
/// endpoint; handlers then narrow: full viewers see everything, everyone else
/// must be assigned to the ticket or share its group.
/// </summary>
internal static class TicketAccess
{
	public static bool HasFullView(ClaimsPrincipal? principal) =>
		principal?.HasClaim(PermissionClaims.Type, TicketPolicyConstants.ViewAllPolicy) == true;

	public static async Task<bool> IsAssignedAsync(
		TicketingDbContext db,
		int ticketId,
		string? userRefId,
		CancellationToken ct)
	{
		if (string.IsNullOrEmpty(userRefId))
			return false;

		return await db.TicketOperators
			.AsNoTracking()
			.AnyAsync(to => to.TicketId == ticketId && to.Operator.RefId == userRefId, ct);
	}

	public static bool InGroup(string? ticketGroupId, string? callerGroupId) =>
		!string.IsNullOrWhiteSpace(callerGroupId)
		&& !string.IsNullOrWhiteSpace(ticketGroupId)
		&& ticketGroupId == callerGroupId;
}
