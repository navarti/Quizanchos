using System.Reflection;
using Quizanchos.WebApi.Services.PluginSystem;
using Xunit;

namespace Quizanchos.WebApi.Tests.PluginSystem;

/// <summary>
/// Unit tests for the assembly-sharing rule of <see cref="PluginLoadContext"/>. To keep descriptor
/// types reference-equal between the host and every plugin, the SDK contract assemblies
/// (Quizanchos.Core / Quizanchos.Common) and the framework assemblies (Microsoft.*, System.*,
/// netstandard, mscorlib) must be deferred to the default load context, while a plugin's own
/// third-party dependencies must be resolved inside its isolated context.
/// </summary>
public class PluginLoadContextTests
{
    private static bool IsHostSharedAssembly(string name)
    {
        var method = typeof(PluginLoadContext).GetMethod(
            "IsHostSharedAssembly", BindingFlags.NonPublic | BindingFlags.Static)!;
        return (bool)method.Invoke(null, [name])!;
    }

    [Theory]
    [InlineData("Quizanchos.Core")]
    [InlineData("Quizanchos.Common")]
    [InlineData("Microsoft.Extensions.Logging.Abstractions")]
    [InlineData("System.Text.Json")]
    [InlineData("netstandard")]
    [InlineData("mscorlib")]
    public void IsHostSharedAssembly_SharedContractsAndFramework_ReturnTrue(string name)
    {
        Assert.True(IsHostSharedAssembly(name));
    }

    [Theory]
    [InlineData("Newtonsoft.Json")]
    [InlineData("AutoMapper")]
    [InlineData("SomeThirdPartyPlugin")]
    [InlineData("CoolMinigame.Engine")]
    public void IsHostSharedAssembly_PluginPrivateDependencies_ReturnFalse(string name)
    {
        Assert.False(IsHostSharedAssembly(name));
    }
}
