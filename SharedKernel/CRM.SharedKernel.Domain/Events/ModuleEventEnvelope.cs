using System.Text.Json;

namespace CRM.SharedKernel.Domain.Events;

/// <summary>
/// The wire format for module events exchanged between modules through the platform.
/// The platform delivers envelopes without interpreting the payload.
/// </summary>
public sealed record ModuleEventEnvelope(
    string EventName,
    Guid EventId,
    DateTime OccurredAt,
    JsonElement Payload);
