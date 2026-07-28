namespace CRM.SharedKernel.API.Tracing;

public sealed class TracingOptions
{
    public string ModuleName { get; set; } = string.Empty;
#pragma warning disable CA1819
    public string[] PathPrefixes { get; set; } = [];
#pragma warning restore CA1819
}
