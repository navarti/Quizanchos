using Microsoft.Extensions.FileProviders;
using Quizanchos.Core;
using System.Reflection;
using System.Runtime.Loader;

namespace Quizanchos.WebApi.Services.PluginSystem;

/// <summary>
/// A plugin that has been successfully loaded from disk. The <see cref="LoadContext"/>
/// reference must be retained for the plugin's lifetime to prevent the assembly from
/// being unloaded.
/// </summary>
public sealed record LoadedPlugin
{
    public required string PluginId { get; init; }
    public required string SourceDirectory { get; init; }
    public required Assembly Assembly { get; init; }
    public required AssemblyLoadContext LoadContext { get; init; }
    public required IReadOnlyList<IMinigameDescriptor> Descriptors { get; init; }
    public required IReadOnlyList<IMinigameFrontendDescriptor> FrontendDescriptors { get; init; }
    public IFileProvider? StaticFiles { get; init; }
}
