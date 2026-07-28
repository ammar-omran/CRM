using CRM.SharedKernel.Domain.Modules;

public class ModuleManifest : CRM.SharedKernel.Domain.Modules.IModuleManifest
{
    public ModuleIdentity Identity { get; set; } = default!;

    public ModuleVersioning Versioning { get; set; } = default!;

    public ModuleClassification Classification { get; set; } = default!;

    public IReadOnlyList<ModuleCapability> ProvidedCapabilities { get; set; } = default!;

    public IReadOnlyList<ModuleCapability>? RequiredCapabilities { get; set; } = default!;

    public IReadOnlyList<ModuleDependency>? ModuleDependencies { get; set; } = default!;

    public ModuleApi? Api { get; set; } = default!;

    public IReadOnlyList<ModulePolicy> Policies { get; set; } = default!;

    public ModuleEvents? Events { get; set; } = default!;

    public ModuleHealth Health { get; set; } = default!;

    public ModuleDatabase Database { get; set; } = default!;

    public IReadOnlyList<ManifestExtension>? Extensions { get; set; } = default!;
}
