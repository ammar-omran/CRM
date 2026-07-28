namespace CRM.SharedKernel.Domain.Modules;

/// <summary>
/// The module manifest contract. Every module must implement this interface.
/// The manifest answers one question: "Can I safely host this module?"
/// <para>
/// The manifest is a composition of smaller immutable records, each representing
/// a distinct platform concern. Modules implement this interface; the platform
/// reads it for discovery, validation, routing, monitoring, and management.
/// </para>
/// </summary>
public interface IModuleManifest
{
  /// <summary>
  /// Unique identification of this module.
  /// </summary>
  ModuleIdentity Identity { get; }

  /// <summary>
  /// Version information for the manifest schema and the module.
  /// </summary>
  ModuleVersioning Versioning { get; }

  /// <summary>
  /// How this module participates in the platform and who it serves.
  /// </summary>
  ModuleClassification Classification { get; }

  /// <summary>
  /// Business capabilities this module provides to the platform.
  /// The platform discovers capabilities, not module internals.
  /// </summary>
  IReadOnlyList<ModuleCapability> ProvidedCapabilities { get; }

  /// <summary>
  /// Capabilities this module requires from other modules.
  /// Null means no capability-level dependencies.
  /// </summary>
  IReadOnlyList<ModuleCapability>? RequiredCapabilities { get; }

  /// <summary>
  /// Module-level dependencies for startup ordering and compatibility validation.
  /// Null means no module dependencies.
  /// </summary>
  IReadOnlyList<ModuleDependency>? ModuleDependencies { get; }

  /// <summary>
  /// API contribution details. Null for non-HTTP modules.
  /// The platform uses this to build the routing table and enforce authentication.
  /// </summary>
  ModuleApi? Api { get; }

  /// <summary>
  /// Authorization policies defined by this module.
  /// Policy names are globally unique and prefixed with the module name.
  /// </summary>
  IReadOnlyList<ModulePolicy> Policies { get; }

  /// <summary>
  /// Event publications and subscriptions. Null for modules that don't participate in event-driven communication.
  /// The platform uses this for event topology visualization and dependency mapping.
  /// </summary>
  ModuleEvents? Events { get; }

  /// <summary>
  /// Health check endpoints exposed by this module.
  /// The module decides what "healthy" means; the platform only needs to know where to check.
  /// </summary>
  ModuleHealth Health { get; }

  /// <summary>
  /// Database ownership information. The module owns its schema.
  /// </summary>
  ModuleDatabase Database { get; }

  /// <summary>
  /// Forward-compatible extension points. The platform ignores extensions it doesn't recognize.
  /// Null means no extensions.
  /// </summary>
  IReadOnlyList<ManifestExtension>? Extensions { get; }
}
