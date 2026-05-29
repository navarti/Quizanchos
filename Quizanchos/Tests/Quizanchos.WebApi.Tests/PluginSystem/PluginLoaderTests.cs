using Microsoft.Extensions.Logging.Abstractions;
using Quizanchos.WebApi.Services.PluginSystem;
using Xunit;

namespace Quizanchos.WebApi.Tests.PluginSystem;

/// <summary>
/// Unit tests for <see cref="PluginLoader"/> — the discovery routine of the author's plugin
/// subsystem. The tests cover the robustness guarantees of directory scanning: a missing or
/// empty plugin root, a plugin folder without an entry assembly, and a malformed manifest must
/// all be tolerated gracefully (an empty result, never an exception that would block startup).
/// Each test uses a throw-away temporary directory and never loads a real assembly.
/// </summary>
public class PluginLoaderTests
{
    private sealed class TempDirectory : IDisposable
    {
        public string Path { get; } =
            System.IO.Path.Combine(System.IO.Path.GetTempPath(), "qz-plugin-tests-" + Guid.NewGuid().ToString("N"));

        public TempDirectory() => Directory.CreateDirectory(Path);

        public string CreateSubdirectory(string name)
        {
            string full = System.IO.Path.Combine(Path, name);
            Directory.CreateDirectory(full);
            return full;
        }

        public void Dispose()
        {
            try { Directory.Delete(Path, recursive: true); }
            catch { /* best-effort cleanup */ }
        }
    }

    [Fact]
    public void LoadFromDirectory_NonexistentRoot_ReturnsEmpty()
    {
        string missing = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "qz-missing-" + Guid.NewGuid().ToString("N"));

        var result = PluginLoader.LoadFromDirectory(missing, NullLogger.Instance);

        Assert.Empty(result);
    }

    [Fact]
    public void LoadFromDirectory_EmptyRoot_ReturnsEmpty()
    {
        using var temp = new TempDirectory();

        var result = PluginLoader.LoadFromDirectory(temp.Path, NullLogger.Instance);

        Assert.Empty(result);
    }

    [Fact]
    public void LoadFromDirectory_PluginFolderWithoutEntryAssembly_IsSkipped()
    {
        using var temp = new TempDirectory();
        temp.CreateSubdirectory("BrokenPlugin");

        var result = PluginLoader.LoadFromDirectory(temp.Path, NullLogger.Instance);

        Assert.Empty(result);
    }

    [Fact]
    public void LoadFromDirectory_MalformedManifest_DoesNotThrow()
    {
        using var temp = new TempDirectory();
        string pluginDir = temp.CreateSubdirectory("PluginWithBadManifest");
        File.WriteAllText(System.IO.Path.Combine(pluginDir, "plugin.json"), "{ this is not valid json");

        // A malformed manifest falls back to defaults; with no entry assembly the folder is skipped,
        // but the scan as a whole must not throw.
        var result = PluginLoader.LoadFromDirectory(temp.Path, NullLogger.Instance);

        Assert.Empty(result);
    }
}
