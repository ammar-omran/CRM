namespace CRM.SharedKernel.API.Tracing;

public sealed class TracingOptions
{
	public string ModuleName { get; set; } = string.Empty;
	public string[] PathPrefixes { get; set; } = [];
}
