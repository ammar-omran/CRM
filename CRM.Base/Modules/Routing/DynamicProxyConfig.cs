using System.Threading;
using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;

namespace CRM.Base.Modules.Routing;

/// <summary>
/// A snapshot of the reverse proxy configuration generated from the module catalog.
/// Immutable — replaced entirely when the catalog changes.
/// </summary>
internal sealed class DynamicProxyConfig : IProxyConfig
{
	/// <summary>
	/// Initializes a new instance with pre-built routes and clusters.
	/// </summary>
	public DynamicProxyConfig(
		string revisionId,
		IReadOnlyList<RouteConfig> routes,
		IReadOnlyList<ClusterConfig> clusters,
		CancellationTokenSource cts)
	{
		RevisionId = revisionId;
		Routes = routes;
		Clusters = clusters;
		ChangeToken = new CancellationChangeToken(cts.Token);
	}

	/// <inheritdoc />
	public string RevisionId { get; }

	/// <inheritdoc />
	public IReadOnlyList<RouteConfig> Routes { get; }

	/// <inheritdoc />
	public IReadOnlyList<ClusterConfig> Clusters { get; }

	/// <inheritdoc />
	public IChangeToken ChangeToken { get; }
}
