namespace Quizanchos.WebApi.Services.PluginSystem;

/// <summary>
/// Optional plugin metadata loaded from <c>plugin.json</c> in a plugin folder.
/// All fields are optional; sensible defaults are derived from the folder name when missing.
/// </summary>
internal sealed record PluginManifest
{
    /// <summary>File name of the entry assembly inside the plugin folder. Defaults to <c>{folderName}.dll</c>.</summary>
    public string? EntryAssembly { get; init; }

    /// <summary>Subfolder containing the plugin's static assets. Defaults to <c>wwwroot</c>.</summary>
    public string? WwwRoot { get; init; }

    /// <summary>Plugin author-declared version. Informational only.</summary>
    public string? Version { get; init; }
}
