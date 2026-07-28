using CRM.SharedKernel.Domain.Modules;

namespace CRM.Base.Modules.DependencyResolution;

/// <summary>
/// Resolves module startup order based on declared dependencies.
/// Detects cycles and missing dependencies.
/// </summary>
public interface IDependencyResolver
{
	/// <summary>
	/// Resolves the startup order for the given manifests using topological sort.
	/// </summary>
	/// <param name="manifests">The manifests to sort.</param>
	/// <returns>An ordered list of manifests respecting dependency constraints.</returns>
	/// <exception cref="DependencyCycleException">Thrown when a dependency cycle is detected.</exception>
	/// <exception cref="MissingDependencyException">Thrown when a required dependency is not in the input set.</exception>
	IReadOnlyList<IModuleManifest> ResolveStartupOrder(IReadOnlyList<IModuleManifest> manifests);
}

/// <summary>
/// Thrown when a dependency cycle is detected in the module dependency graph.
/// </summary>
public sealed class DependencyCycleException(string message) : Exception(message);

/// <summary>
/// Thrown when a required dependency is missing from the available modules.
/// </summary>
public sealed class MissingDependencyException(string message) : Exception(message);
