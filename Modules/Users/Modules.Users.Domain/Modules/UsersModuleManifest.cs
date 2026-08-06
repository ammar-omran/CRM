using CRM.SharedKernel.Domain.Modules;

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
		}
	];

	/// <inheritdoc />
	public IReadOnlyList<ModuleCapability>? RequiredCapabilities { get; } = null;

	/// <inheritdoc />
	public IReadOnlyList<ModuleDependency>? ModuleDependencies { get; } = null;

	/// <inheritdoc />
	public ModuleApi? Api { get; } = new()
	{
		RoutePrefixs = ["/api/users"],
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
	[
		new ModulePolicy { Name = "users:read", Description = "Allows reading user information." },
		new ModulePolicy { Name = "users:create", Description = "Allows creating new users." },
		new ModulePolicy { Name = "users:update", Description = "Allows updating user information." },
		new ModulePolicy { Name = "users:delete", Description = "Allows deleting users." }
	];

	/// <inheritdoc />
	public ModuleEvents? Events { get; } = null;

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
