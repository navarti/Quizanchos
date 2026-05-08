namespace Quizanchos.WebApi.Services.PluginSystem;

/// <summary>
/// Read-only catalog of plugins loaded from the plugin root at startup.
/// Registered as a singleton so middleware setup can mount each plugin's static files.
/// </summary>
public interface IPluginCatalog
{
    IReadOnlyList<LoadedPlugin> Plugins { get; }
}

internal sealed class PluginCatalog : IPluginCatalog
{
    public PluginCatalog(IReadOnlyList<LoadedPlugin> plugins)
    {
        Plugins = plugins;
    }

    public IReadOnlyList<LoadedPlugin> Plugins { get; }
}
