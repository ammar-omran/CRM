namespace CRM.Base.Modules.Admin.Permissions;

/// <summary>
/// A permission exposed by a registered module, surfaced in the Super Admin UI as a
/// candidate claim that can be granted to a Portal module's roles. Cross-module grants
/// (e.g. giving a Users role the Ticketing permission <c>crm.ticketing:ticket:create</c>)
/// are expressed through <see cref="FullPermission"/>.
/// </summary>
public sealed class ExposedPermission
{
	public required string SourceModuleId { get; init; }
	public required string SourceModuleName { get; init; }

	/// <summary>The module's own policy name, e.g. <c>ticket:create</c>.</summary>
	public required string Policy { get; init; }

	/// <summary>Fully-qualified permission used as the claim type, e.g. <c>crm.ticketing:ticket:create</c>.</summary>
	public required string FullPermission { get; init; }

	public string? Description { get; init; }
}
