using Microsoft.Extensions.FileProviders;
using Quizanchos.Core;
using System.Reflection;
using System.Text.Json;

namespace Quizanchos.WebApi.Services.PluginSystem;

/// <summary>
/// Scans a plugin root directory and loads each subdirectory as an isolated plugin.
/// </summary>
internal static class PluginLoader
{
    private const int MinThirdPartyMinigameTypeId = 1000;

    public static IReadOnlyList<LoadedPlugin> LoadFromDirectory(string pluginRoot, ILogger logger)
    {
        if (!Directory.Exists(pluginRoot))
        {
            logger.LogInformation("Plugin root '{Root}' does not exist; no third-party plugins loaded.", pluginRoot);
            return Array.Empty<LoadedPlugin>();
        }

        var loaded = new List<LoadedPlugin>();
        foreach (var pluginDir in Directory.EnumerateDirectories(pluginRoot))
        {
            var folderName = Path.GetFileName(pluginDir);
            try
            {
                var plugin = LoadOne(pluginDir, folderName, logger);
                if (plugin is not null)
                {
                    loaded.Add(plugin);
                    logger.LogInformation(
                        "Loaded plugin '{PluginId}' ({DescriptorCount} backend, {FrontendCount} frontend descriptors) from {Path}",
                        plugin.PluginId, plugin.Descriptors.Count, plugin.FrontendDescriptors.Count, pluginDir);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to load plugin from {Path}", pluginDir);
            }
        }

        return loaded;
    }

    private static LoadedPlugin? LoadOne(string pluginDir, string folderName, ILogger logger)
    {
        var manifest = ReadManifest(pluginDir, logger);
        var entryName = manifest?.EntryAssembly ?? $"{folderName}.dll";
        var entryPath = Path.Combine(pluginDir, entryName);

        if (!File.Exists(entryPath))
        {
            logger.LogWarning("Plugin folder '{Folder}' has no entry assembly '{Entry}'", folderName, entryName);
            return null;
        }

        var loadContext = new PluginLoadContext(entryPath, folderName);
        var assembly = loadContext.LoadFromAssemblyPath(entryPath);

        var descriptors = InstantiateAll<IMinigameDescriptor>(assembly);
        var frontendDescriptors = InstantiateAll<IMinigameFrontendDescriptor>(assembly);

        if (descriptors.Count == 0 && frontendDescriptors.Count == 0)
        {
            logger.LogWarning("Plugin '{Folder}' loaded but contained no descriptors", folderName);
        }

        foreach (var descriptor in descriptors)
        {
            if (descriptor.MinigameTypeId < MinThirdPartyMinigameTypeId)
            {
                throw new InvalidOperationException(
                    $"Third-party plugin '{folderName}' descriptor '{descriptor.GetType().FullName}' " +
                    $"uses MinigameTypeId={descriptor.MinigameTypeId}, but third-party IDs must be >= {MinThirdPartyMinigameTypeId}.");
            }
        }

        var wwwRootSubpath = manifest?.WwwRoot ?? "wwwroot";
        var wwwRootFull = Path.Combine(pluginDir, wwwRootSubpath);
        IFileProvider? fileProvider = Directory.Exists(wwwRootFull)
            ? new PhysicalFileProvider(wwwRootFull)
            : null;

        return new LoadedPlugin
        {
            PluginId = folderName,
            SourceDirectory = pluginDir,
            Assembly = assembly,
            LoadContext = loadContext,
            Descriptors = descriptors,
            FrontendDescriptors = frontendDescriptors,
            StaticFiles = fileProvider,
        };
    }

    private static PluginManifest? ReadManifest(string pluginDir, ILogger logger)
    {
        var manifestPath = Path.Combine(pluginDir, "plugin.json");
        if (!File.Exists(manifestPath))
        {
            return null;
        }

        try
        {
            var json = File.ReadAllText(manifestPath);
            return JsonSerializer.Deserialize<PluginManifest>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to read plugin manifest at {Path}; using defaults", manifestPath);
            return null;
        }
    }

    private static IReadOnlyList<T> InstantiateAll<T>(Assembly assembly) where T : class
    {
        Type[] types;
        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            types = ex.Types.Where(t => t is not null).Cast<Type>().ToArray();
        }

        return types
            .Where(t => t is { IsInterface: false, IsAbstract: false } && typeof(T).IsAssignableFrom(t))
            .Select(t => Activator.CreateInstance(t) as T)
            .Where(d => d is not null)
            .Select(d => d!)
            .ToArray();
    }
}
