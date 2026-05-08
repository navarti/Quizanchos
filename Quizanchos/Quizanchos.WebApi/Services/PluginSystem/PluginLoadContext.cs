using System.Reflection;
using System.Runtime.Loader;

namespace Quizanchos.WebApi.Services.PluginSystem;

/// <summary>
/// Isolated <see cref="AssemblyLoadContext"/> for a single plugin. Resolves the plugin's
/// transitive dependencies via its .deps.json, but defers SDK contract assemblies and
/// framework assemblies to the default context so descriptor types are reference-equal
/// across the host and all plugins.
/// </summary>
internal sealed class PluginLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    public PluginLoadContext(string pluginEntryPath, string name)
        : base(name: name, isCollectible: false)
    {
        _resolver = new AssemblyDependencyResolver(pluginEntryPath);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        if (assemblyName.Name is { } name && IsHostSharedAssembly(name))
        {
            return null;
        }

        var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        return assemblyPath is not null ? LoadFromAssemblyPath(assemblyPath) : null;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        return libraryPath is not null ? LoadUnmanagedDllFromPath(libraryPath) : IntPtr.Zero;
    }

    private static bool IsHostSharedAssembly(string name)
    {
        return name.StartsWith("Quizanchos.Core", StringComparison.OrdinalIgnoreCase)
            || name.StartsWith("Quizanchos.Common", StringComparison.OrdinalIgnoreCase)
            || name.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase)
            || name.StartsWith("System.", StringComparison.OrdinalIgnoreCase)
            || name.Equals("netstandard", StringComparison.OrdinalIgnoreCase)
            || name.Equals("mscorlib", StringComparison.OrdinalIgnoreCase);
    }
}
