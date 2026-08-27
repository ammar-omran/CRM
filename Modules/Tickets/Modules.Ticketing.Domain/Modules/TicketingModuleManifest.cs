using System.Reflection;
using CRM.SharedKernel.Domain.Modules;
using Modules.Ticketing.Domain.Policies;

namespace Modules.Ticketing.Domain.Modules;

/// <summary>
/// Module manifest for the Ticketing module.
/// Provides ticket management, agent assignment, and customer support capabilities.
/// </summary>
public sealed class TicketingModuleManifest : IModuleManifest
{
	/// <inheritdoc />
	public ModuleIdentity Identity { get; } = new()
	{
		ModuleId = "crm.ticketing",
		DisplayName = "Ticketing",
		Description = "Ticket management, agent assignment, and customer support.",
		Author = "Azka",
		Tags = ["tickets", "support", "shared"]
	};

	/// <inheritdoc />
	public ModuleVersioning Versioning { get; } = new()
	{
		ManifestVersion = "1.0.0",
		ModuleVersion = "1.0.0",
		MinPlatformVersion = "1.0.0",
		MinSharedKernelVersion = "1.0.0"
	};

	/// <inheritdoc />
	public ModuleClassification Classification { get; } = new()
	{
		Kind = ModuleKind.Domain,
		Audience = ModuleAudience.Shared
	};

	/// <inheritdoc />
	public IReadOnlyList<ModuleCapability> ProvidedCapabilities { get; } =
	[
		new ModuleCapability
		{
			CapabilityId = "ticket-management",
			DisplayName = "Ticket Management",
			Description = "Create, track, and manage support tickets.",
			Category = "Ticketing"
		},
		new ModuleCapability
		{
			CapabilityId = "ticket-attachments",
			DisplayName = "Ticket Attachments",
			Description = "File attachment management for tickets.",
			Category = "Ticketing"
		}
	];

	/// <inheritdoc />
	public IReadOnlyList<ModuleCapability>? RequiredCapabilities { get; } = null;

	/// <inheritdoc />
	public IReadOnlyList<ModuleDependency>? ModuleDependencies { get; } = null;

	/// <inheritdoc />
	public ModuleApi? Api { get; } = new()
	{
		RoutePrefixs = ["/api/tickets", "/api/attachments"],
		OpenApiEndpoint = "/swagger/v1/swagger.json"
	};

	/// <inheritdoc />
	public IReadOnlyList<ModulePolicy> Policies { get; } =
		PolicyConstants.FromConstants(
			typeof(TicketPolicyConstants)
			// Add more entity constants classes here as the module grows, e.g. typeof(TicketCommentPolicyConstants)
		);

	/// <inheritdoc />
	public ModuleEvents? Events { get; } = new()
	{
		Published = ["ticketing.ticket-created"]
	};

	/// <inheritdoc />
	public ModuleHealth Health { get; } = new()
	{
		Endpoint = "/health",
		Liveness = "/alive"
	};

	/// <inheritdoc />
	public ModuleDatabase Database { get; } = new()
	{
		SchemaName = "ticketing"
	};

	/// <inheritdoc />
	public IReadOnlyList<ManifestExtension>? Extensions { get; } = null;
}
