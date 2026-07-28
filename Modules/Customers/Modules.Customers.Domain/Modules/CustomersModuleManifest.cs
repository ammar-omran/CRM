using CRM.SharedKernel.Domain.Modules;

namespace Modules.Customers.Domain.Modules;

/// <summary>
/// Module manifest for the Customers module.
/// Provides customer-facing portal with authentication, registration, and account management.
/// </summary>
public sealed class CustomersModuleManifest : IModuleManifest
{
	/// <inheritdoc />
	public ModuleIdentity Identity { get; } = new()
	{
		ModuleId = "crm.customers",
		DisplayName = "Customers",
		Description = "Customer-facing portal with authentication, registration, and account management.",
		Author = "Azka",
		Tags = ["customers", "portal", "external"]
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
		Audience = ModuleAudience.External
	};

	/// <inheritdoc />
	public IReadOnlyList<ModuleCapability> ProvidedCapabilities { get; } =
	[
		new ModuleCapability
		{
			CapabilityId = "customer-identity",
			DisplayName = "Customer Identity",
			Description = "Customer authentication, registration, and account management.",
			Category = "Identity"
		},
		new ModuleCapability
		{
			CapabilityId = "customer-portal",
			DisplayName = "Customer Portal",
			Description = "Customer-facing API for self-service operations.",
			Category = "Portal"
		}
	];

	/// <inheritdoc />
	public IReadOnlyList<ModuleCapability>? RequiredCapabilities { get; } = null;

	/// <inheritdoc />
	public IReadOnlyList<ModuleDependency>? ModuleDependencies { get; } = null;

	/// <inheritdoc />
	public ModuleApi? Api { get; } = new()
	{
		RoutePrefix = "/api/customers",
		OpenApiEndpoint = "/swagger/v1/swagger.json",
		AnonymousPaths =
		[
			"/api/customers/login",
			"/api/customers/login-phone",
			"/api/customers/register",
			"/api/customers/confirm-email",
			"/api/customers/resend-otp",
			"/api/customers/update-password",
			"/api/customers/reset-password"
		]
	};

	/// <inheritdoc />
	public IReadOnlyList<ModulePolicy> Policies { get; } = [];

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
		SchemaName = "customers"
	};

	/// <inheritdoc />
	public IReadOnlyList<ManifestExtension>? Extensions { get; } = null;
}
