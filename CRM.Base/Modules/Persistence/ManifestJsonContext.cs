using CRM.Base.Modules.Api;
using System.Text.Json.Serialization;

namespace CRM.Base.Modules.Persistence;

/// <summary>
/// Source-generated JSON context for serializing IModuleManifest.
/// Ensures reliable serialization without runtime reflection.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    UseStringEnumConverter = true)]
[JsonSerializable(typeof(ModuleManifest))]
[JsonSerializable(typeof(CRM.SharedKernel.Domain.Modules.ModuleIdentity))]
[JsonSerializable(typeof(CRM.SharedKernel.Domain.Modules.ModuleVersioning))]
[JsonSerializable(typeof(CRM.SharedKernel.Domain.Modules.ModuleClassification))]
[JsonSerializable(typeof(CRM.SharedKernel.Domain.Modules.ModuleCapability))]
[JsonSerializable(typeof(CRM.SharedKernel.Domain.Modules.ModuleDependency))]
[JsonSerializable(typeof(CRM.SharedKernel.Domain.Modules.ModulePolicy))]
[JsonSerializable(typeof(CRM.SharedKernel.Domain.Modules.ModuleApi))]
[JsonSerializable(typeof(CRM.SharedKernel.Domain.Modules.ModuleEvents))]
[JsonSerializable(typeof(CRM.SharedKernel.Domain.Modules.ModuleHealth))]
[JsonSerializable(typeof(CRM.SharedKernel.Domain.Modules.ModuleDatabase))]
[JsonSerializable(typeof(CRM.SharedKernel.Domain.Modules.ManifestExtension))]
[JsonSerializable(typeof(CRM.SharedKernel.Domain.Modules.ModuleKind))]
[JsonSerializable(typeof(CRM.SharedKernel.Domain.Modules.ModuleAudience))]
[JsonSerializable(typeof(RegisterModuleRequest))]
[JsonSerializable(typeof(RegisterModuleResponse))]
[JsonSerializable(typeof(List<ModuleListItem>))]
public partial class ManifestJsonContext : JsonSerializerContext;
