namespace Modules.Ticketing.Domain.Entities;

public class TicketOperator
{
	public int Id { get; set; }
	public int TicketId { get; set; }
	public int OperatorId { get; set; }

	/// <summary>
	/// Persisted as formatted string "{operationRole}:{userRolesCsv}".
	/// e.g. "creator:Admin;Support" or "assignee:Agent".
	/// Back-compat: legacy plain operation role without ':' is treated as operation role with empty user roles.
	/// </summary>
	public string Role { get; private set; } = string.Empty;
	public Ticket Ticket { get; set; } = default!;
	public Operator Operator { get; set; } = default!;

	// Value-object accessors
	public string OperatorRole
	{
		get
		{
			if (string.IsNullOrWhiteSpace(Role))
				return string.Empty;
			var idx = Role.IndexOf(':');
			return idx < 0 ? Role.Trim() : Role[..idx].Trim();
		}
	}

	public IReadOnlyList<string> UserRoles
	{
		get
		{
			if (string.IsNullOrWhiteSpace(Role))
				return Array.Empty<string>();
			var idx = Role.IndexOf(':');
			if (idx < 0)
				return Array.Empty<string>();
			var csv = Role[(idx + 1)..];
			return string.IsNullOrWhiteSpace(csv)
				? Array.Empty<string>()
				: csv.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		}
	}

	public TicketOperationRole? OperationRoleEnum
	{
		get
		{
			if (Enum.TryParse<TicketOperationRole>(OperatorRole, true, out var parsed))
				return parsed;
			return null;
		}
	}

	public void SetRole(string operationRole, IEnumerable<string>? userRoles)
	{
		if (string.IsNullOrWhiteSpace(operationRole))
			throw new ArgumentException("Operation role is required.", nameof(operationRole));

		operationRole = operationRole.Trim().ToLowerInvariant();

		// Validate against enum when possible, but allow custom if needed (forward-compat).
		// We normalise to enum lower-case when it matches.
		if (Enum.TryParse<TicketOperationRole>(operationRole, true, out var parsed))
			operationRole = parsed.ToString().ToLowerInvariant();

		var csv = userRoles is null ? string.Empty : string.Join(';', userRoles.Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => r.Trim()).Distinct(StringComparer.OrdinalIgnoreCase));
		Role = $"{operationRole}:{csv}";
	}

	public void SetRole(TicketOperationRole operationRole, IEnumerable<string>? userRoles)
		=> SetRole(operationRole.ToString().ToLowerInvariant(), userRoles);

	/// <summary>EF + migration compat — bypasses formatting. Avoid in domain code.</summary>
	public void SetRawRole(string raw) => Role = raw ?? string.Empty;
}

public enum TicketOperationRole
{
	Creator = 0,
	Assignee = 1,
	Watcher = 3
}
