using CRM.SharedKernel.Domain.Modules;

namespace CRM.Base.Modules.DependencyResolution;

/// <summary>
/// Resolves module startup order using topological sort (Kahn's algorithm).
/// Modules with no dependencies start first; dependent modules start after their dependencies.
/// </summary>
public sealed class DependencyResolver : IDependencyResolver
{
	/// <inheritdoc />
	public IReadOnlyList<IModuleManifest> ResolveStartupOrder(IReadOnlyList<IModuleManifest> manifests)
	{
		ArgumentNullException.ThrowIfNull(manifests);

		if (manifests.Count == 0)
		{
			return [];
		}

		// Build adjacency list and in-degree map
		var graph = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
		var inDegree = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
		var manifestMap = new Dictionary<string, IModuleManifest>(StringComparer.OrdinalIgnoreCase);

		foreach (var manifest in manifests)
		{
			var moduleId = manifest.Identity.ModuleId;
			manifestMap[moduleId] = manifest;
			graph.TryAdd(moduleId, []);
			inDegree.TryAdd(moduleId, 0);
		}

		// Validate all dependencies exist in the input set
		foreach (var manifest in manifests)
		{
			if (manifest.ModuleDependencies is null)
			{
				continue;
			}

			foreach (var dep in manifest.ModuleDependencies)
			{
				if (!manifestMap.ContainsKey(dep.ModuleId))
				{
					if (dep.IsRequired)
					{
						throw new MissingDependencyException(
							$"Module '{manifest.Identity.ModuleId}' requires dependency " +
							$"'{dep.ModuleId}', which is not available.");
					}

					// Skip optional missing dependencies
					continue;
				}

				// dep.ModuleId → manifest.Identity.ModuleId (dep must start first)
				graph[dep.ModuleId].Add(manifest.Identity.ModuleId);
				inDegree[manifest.Identity.ModuleId]++;
			}
		}

		// Kahn's algorithm
		var queue = new Queue<string>(
			inDegree.Where(kv => kv.Value == 0).Select(kv => kv.Key));
		var sorted = new List<string>();

		while (queue.Count > 0)
		{
			var current = queue.Dequeue();
			sorted.Add(current);

			foreach (var neighbor in graph[current])
			{
				inDegree[neighbor]--;
				if (inDegree[neighbor] == 0)
				{
					queue.Enqueue(neighbor);
				}
			}
		}

		// If not all modules were processed, there's a cycle
		if (sorted.Count != manifests.Count)
		{
			var cycleModules = manifestMap.Keys
				.Where(id => !sorted.Contains(id))
				.ToList();

			throw new DependencyCycleException(
				$"Dependency cycle detected involving modules: {string.Join(", ", cycleModules)}");
		}

		return sorted.Select(id => manifestMap[id]).ToList().AsReadOnly();
	}
}
