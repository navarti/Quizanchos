using System.Text.Json;
using Quizanchos.WebApi.Services.PluginSystem;
using Xunit;

namespace Quizanchos.WebApi.Tests.PluginSystem;

/// <summary>
/// Unit tests for the <c>plugin.json</c> manifest model used by the plugin loader. The loader
/// deserializes the manifest case-insensitively and treats every field as optional, deriving
/// sensible defaults from the folder name when a field is absent. These tests pin that contract.
/// </summary>
public class PluginManifestTests
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    private static PluginManifest? Deserialize(string json) =>
        JsonSerializer.Deserialize<PluginManifest>(json, Options);

    [Fact]
    public void Deserialize_FullManifest_PopulatesEveryField()
    {
        const string json = """
            { "entryAssembly": "MyGame.dll", "wwwRoot": "assets", "version": "1.2.3" }
            """;

        var manifest = Deserialize(json);

        Assert.NotNull(manifest);
        Assert.Equal("MyGame.dll", manifest!.EntryAssembly);
        Assert.Equal("assets", manifest.WwwRoot);
        Assert.Equal("1.2.3", manifest.Version);
    }

    [Fact]
    public void Deserialize_IsCaseInsensitive()
    {
        const string json = """{ "EntryAssembly": "Pascal.dll", "WWWROOT": "static" }""";

        var manifest = Deserialize(json);

        Assert.NotNull(manifest);
        Assert.Equal("Pascal.dll", manifest!.EntryAssembly);
        Assert.Equal("static", manifest.WwwRoot);
    }

    [Fact]
    public void Deserialize_EmptyObject_LeavesAllFieldsNullForDefaulting()
    {
        var manifest = Deserialize("{}");

        Assert.NotNull(manifest);
        Assert.Null(manifest!.EntryAssembly);
        Assert.Null(manifest.WwwRoot);
        Assert.Null(manifest.Version);
    }

    [Fact]
    public void Deserialize_PartialManifest_LeavesOmittedFieldsNull()
    {
        const string json = """{ "wwwRoot": "public" }""";

        var manifest = Deserialize(json);

        Assert.NotNull(manifest);
        Assert.Null(manifest!.EntryAssembly);
        Assert.Equal("public", manifest.WwwRoot);
    }
}
