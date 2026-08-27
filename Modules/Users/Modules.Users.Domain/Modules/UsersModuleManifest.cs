using System.Reflection;
using CRM.SharedKernel.Domain.Modules;
using Modules.Users.Domain.Policies;

namespace Modules.Users.Domain.Modules;

/// <summary>
/// Module manifest for the Users module.
/// Provides workforce identity, authentication, and user management capabilities.
/// </summary>
public sealed class UsersModuleManifest : IModuleManifest
{
	/// <inheritdoc />
	public ModuleIdentity Identity { get; } = new()
	{
		ModuleId = "crm.users",
		DisplayName = "Users",
		Description = "Workforce identity, authentication, and user management.",
		Author = "Azka",
		Tags = ["identity", "authentication", "rbac"]
	};

	/// <inheritdoc />
	public ModuleVersioning Versioning { get; } = new()
	{
		ManifestVersion = "1.0.0",
		ModuleVersion = "1.0.1",
		MinPlatformVersion = "1.0.0",
		MinSharedKernelVersion = "1.0.0"
	};

	/// <inheritdoc />
	public ModuleClassification Classification { get; } = new()
	{
		Kind = ModuleKind.Portal,
		Audience = ModuleAudience.Internal
	};

	/// <inheritdoc />
	public IReadOnlyList<ModuleCapability> ProvidedCapabilities { get; } =
	[
		new ModuleCapability
		{
			CapabilityId = "workforce-identity",
			DisplayName = "Workforce Identity",
			Description = "User authentication, registration, and token management.",
			Category = "Identity"
		},
		new ModuleCapability
		{
			CapabilityId = "rbac",
			DisplayName = "Role-Based Access Control",
			Description = "User roles and permission management.",
			Category = "Authorization"
		},
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
	public IReadOnlyList<ModuleDependency>? ModuleDependencies { get; } = [
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
		RoutePrefixs = ["/api/users", "/api/organizations", "/api/agents"],
		OpenApiEndpoint = "/swagger/v1/swagger.json",
		AnonymousPaths =
		[
			"/api/users/login",
			"/api/users/register",
			"/api/users/refresh"
		]
	};

	/// <inheritdoc />
	public IReadOnlyList<ModulePolicy> Policies { get; } =
		PolicyConstants.FromConstants(
			typeof(UserPolicyConstants)
			// Add more entity constants classes here as the module grows, e.g. typeof(OrganizationPolicyConstants)
		);
	/// <inheritdoc />
	public ModuleEvents? Events { get; } = new()
	{
		Published = ["users.organization-created"],
		Subscribed = ["ticketing.ticket-created"]
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
		SchemaName = "users"
	};

	/// <inheritdoc />
	public IReadOnlyList<ManifestExtension>? Extensions { get; } = null;
}
