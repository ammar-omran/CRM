using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using CRM.Base.Modules.State;
using CRM.SharedKernel.Domain.Modules;
using Microsoft.Extensions.Logging;

namespace CRM.Base.Modules.Persistence;

/// <summary>
/// The runtime catalog of all loaded modules.
/// Combines SQLite persistence with an in-memory dictionary for fast reads.
/// Loads from SQLite on startup; all mutations are persisted and reflected in-memory.
/// </summary>
public sealed class ModuleCatalog
{
    private readonly ConcurrentDictionary<string, CatalogEntry> _entries = new(StringComparer.OrdinalIgnoreCase);
    private readonly IModulePersistence _persistence;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ModuleCatalog> _logger;

    /// <summary>
    /// Initializes the catalog with required dependencies.
    /// </summary>
    public ModuleCatalog(
        IModulePersistence persistence,
        IHttpClientFactory httpClientFactory,
        ILogger<ModuleCatalog> logger)
    {
        _persistence = persistence;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Callback invoked when the catalog changes (module registered, removed, or loaded).
    /// Used by the dynamic proxy config provider to signal YARP to reload routes.
    /// </summary>
    public Action? OnChanged { get; set; }

    /// <summary>
    /// Gets all loaded catalog entries.
    /// </summary>
    public IReadOnlyList<CatalogEntry> GetAll() => _entries.Values.ToList().AsReadOnly();

    /// <summary>
    /// Gets a catalog entry by module ID.
    /// </summary>
    public CatalogEntry? Get(string moduleId)
    {
        _entries.TryGetValue(moduleId, out var entry);
        return entry;
    }

    /// <summary>
    /// Loads all registered modules from SQLite into the in-memory catalog.
    /// For each module, refreshes the manifest from the module's URL.
    /// If a module is offline, keeps the cached manifest and marks it unavailable.
    /// </summary>
    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Loading registered modules from database...");

        var entities = await _persistence.LoadAllAsync();

        _logger.LogInformation("Found {Count} registered modules in database", entities.Count);

        foreach (var entity in entities)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var entry = await LoadEntryAsync(entity, cancellationToken);

            _entries[entity.ModuleId] = entry;

            if (entry.IsAvailable)
            {
                _logger.LogInformation(
                    "Module '{ModuleId}' loaded successfully from {BaseUrl}",
                    entity.ModuleId, entity.BaseUrl);
            }
            else
            {
                _logger.LogWarning(
                    "Module '{ModuleId}' at {BaseUrl} is unavailable. Using cached manifest.",
                    entity.ModuleId, entity.BaseUrl);
            }
        }

        _logger.LogInformation("Module catalog loaded with {Available}/{Total} modules available",
            _entries.Values.Count(e => e.IsAvailable), entities.Count);

        OnChanged?.Invoke();
    }

    /// <summary>
    /// Registers a new module by fetching its manifest from the given URL.
    /// </summary>
    /// <param name="baseUrl">The module's base URL (e.g., "https://localhost:5003").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The catalog entry, or null if registration failed.</returns>
    public async Task<CatalogEntry?> RegisterAsync(string baseUrl, CancellationToken cancellationToken = default)
    {
        // Normalize URL
        baseUrl = baseUrl.TrimEnd('/');

        // Fetch manifest
        var manifest = await FetchManifestAsync(baseUrl, cancellationToken);
        if (manifest is null)
        {
            return null;
        }

        var moduleId = manifest.Identity?.ModuleId ?? "";

        // Check for duplicates
        if (_entries.ContainsKey(moduleId))
        {
            _logger.LogError("Module '{ModuleId}' is already registered", moduleId);
            return null;
        }

        // Persist to SQLite
        var entity = new RegisteredModuleEntity
        {
            ModuleId = moduleId,
            DisplayName = manifest.Identity.DisplayName,
            BaseUrl = baseUrl,
            ManifestJson = JsonSerializer.Serialize(manifest, ManifestJsonContext.Default.ModuleManifest),
            Enabled = true,
            RegisteredAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _persistence.SaveAsync(entity);

        // Add to in-memory catalog
        var entry = new CatalogEntry
        {
            Entity = entity,
            Manifest = manifest,
            State = ModuleState.Registered,
            IsAvailable = true
        };

        _entries[moduleId] = entry;

        _logger.LogInformation("Module '{ModuleId}' registered from {BaseUrl}", moduleId, baseUrl);

        OnChanged?.Invoke();

        return entry;
    }

    /// <summary>
    /// Removes a module from the catalog and the database.
    /// </summary>
    public async Task<bool> RemoveAsync(string moduleId)
    {
        if (!_entries.TryRemove(moduleId, out _))
        {
            return false;
        }

        await _persistence.RemoveAsync(moduleId);

        _logger.LogInformation("Module '{ModuleId}' removed", moduleId);

        OnChanged?.Invoke();

        return true;
    }

    /// <summary>
    /// Updates the state of a module in the catalog.
    /// </summary>
    public void UpdateState(string moduleId, ModuleState state)
    {
        if (_entries.TryGetValue(moduleId, out var entry))
        {
            entry.State = state;
        }
    }

    private async Task<CatalogEntry> LoadEntryAsync(RegisteredModuleEntity entity, CancellationToken cancellationToken)
    {
        var entry = new CatalogEntry
        {
            Entity = entity,
            Manifest = null,
            State = ModuleState.Discovered,
            IsAvailable = false
        };

        try
        {
            var manifest = await FetchManifestAsync(entity.BaseUrl, cancellationToken);

            if (manifest is not null)
            {
                entry.Manifest = manifest;
                entry.IsAvailable = true;
                entry.State = ModuleState.Registered;

                // Update the cached manifest in the database
                entity.ManifestJson = JsonSerializer.Serialize(manifest, ManifestJsonContext.Default.ModuleManifest);
                entity.UpdatedAt = DateTime.UtcNow;
                await _persistence.UpdateAsync(entity);
            }
            else
            {
                // Use cached manifest from database
                entry.Manifest = DeserializeCachedManifest(entity.ManifestJson);
                entry.State = entity.Enabled ? ModuleState.Failed : ModuleState.Disabled;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to refresh manifest for module '{ModuleId}' at {BaseUrl}. Using cached manifest.",
                entity.ModuleId, entity.BaseUrl);

            entry.Manifest = DeserializeCachedManifest(entity.ManifestJson);
            entry.State = ModuleState.Failed;
        }

        return entry;
    }

    private async Task<IModuleManifest?> FetchManifestAsync(string baseUrl, CancellationToken cancellationToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);

            var url = $"{baseUrl}/module/manifest";
            var response = await client.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Module at {BaseUrl} returned {StatusCode} from /module/manifest",
                    baseUrl, response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            var manifest = JsonSerializer.Deserialize(json, ManifestJsonContext.Default.ModuleManifest);

            return manifest;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to fetch manifest from {BaseUrl}/module/manifest",
                baseUrl);
            return null;
        }
    }

    private static IModuleManifest? DeserializeCachedManifest(string json)
    {
        try
        {
            return JsonSerializer.Deserialize(json, ManifestJsonContext.Default.ModuleManifest);
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// Represents a module entry in the runtime catalog.
/// Combines the persisted entity with the deserialized manifest and runtime state.
/// </summary>
public sealed class CatalogEntry
{
    /// <summary>
    /// The persisted database entity.
    /// </summary>
    public required RegisteredModuleEntity Entity { get; init; }

    /// <summary>
    /// The deserialized manifest. Null if the module is offline and no cached manifest exists.
    /// </summary>
    public IModuleManifest? Manifest { get; set; }

    /// <summary>
    /// The current runtime state.
    /// </summary>
    public ModuleState State { get; set; } = ModuleState.Discovered;

    /// <summary>
    /// Whether the module is currently reachable.
    /// </summary>
    public bool IsAvailable { get; set; }
}
