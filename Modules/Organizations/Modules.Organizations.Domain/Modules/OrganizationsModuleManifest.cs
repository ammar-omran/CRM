using CRM.SharedKernel.Domain.Modules;

namespace Modules.Organizations.Domain.Modules;

/// <summary>
/// Module manifest for the Organizations module.
/// Provides organization management, agent management, and customer tracking capabilities.
/// </summary>
public sealed class OrganizationsModuleManifest : IModuleManifest
{
	/// <inheritdoc />
	public ModuleIdentity Identity { get; } = new()
	{
		ModuleId = "crm.organizations",
		DisplayName = "Organizations",
		Description = "Organization management, agent assignment, and customer tracking.",
		Author = "Azka",
		Tags = ["organizations", "agents", "customers"]
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
		Audience = ModuleAudience.Internal
	};

	/// <inheritdoc />
	public IReadOnlyList<ModuleCapability> ProvidedCapabilities { get; } =
	[
		new ModuleCapability
		{
			CapabilityId = "organization-management",
			DisplayName = "Organization Management",
			Description = "Create, read, and manage organizations.",
			Category = "Organization"
		},
		new ModuleCapability
		{
			CapabilityId = "agent-management",
			DisplayName = "Agent Management",
			Description = "Assign and manage agents within organizations.",
			Category = "Organization"
		}
	];

	/// <inheritdoc />
	public IReadOnlyList<ModuleCapability>? RequiredCapabilities { get; } = null;

	/// <inheritdoc />
	public IReadOnlyList<ModuleDependency>? ModuleDependencies { get; } =
	[
		new ModuleDependency
		{
			ModuleId = "crm.customers",
			IsRequired = false,
			Purpose = "Fetch customer data for organization-customer relationships."
		}
	];

	/// <inheritdoc />
	public ModuleApi? Api { get; } = new()
	{
		RoutePrefixs = ["/api/organizations", "/api/agents"],
		OpenApiEndpoint = "/swagger/v1/swagger.json"
	};

	/// <inheritdoc />
	public IReadOnlyList<ModulePolicy> Policies { get; } = [];

	/// <inheritdoc />
	public ModuleEvents? Events { get; } = new()
	{
		Published = ["OrganizationCreatedEvent"]
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
		SchemaName = "org"
	};

	/// <inheritdoc />
	public IReadOnlyList<ManifestExtension>? Extensions { get; } = null;
}
