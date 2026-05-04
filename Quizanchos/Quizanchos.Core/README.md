# Quizanchos.Core

Plugin SDK for the **Quizanchos** minigame platform.

Implement a few interfaces and your DLL becomes a runtime-loadable minigame: the host discovers it at startup, mounts your static assets, and routes game moves through your logic.

## Install

```bash
dotnet add package Quizanchos.Core
```

`Quizanchos.Common` is pulled in transitively. To keep the host's copy of the SDK authoritative at runtime (avoids type-identity issues and reduces plugin size), reference it like this:

```xml
<PackageReference Include="Quizanchos.Core" Version="0.1.0"
                  PrivateAssets="all" ExcludeAssets="runtime" />
```

## Minimal plugin

```csharp
using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Quizanchos.Core;

public sealed record MyMove : GameMove;

public sealed class MyState : IGameState
{
    public int MinigameType => 1000;
    public Guid GameId { get; set; }
    public IReadOnlyList<string> Players { get; set; } = [];
    public bool IsFinished { get; set; }
    public string? Winner { get; set; }
}

public sealed class MyDescriptor : IMinigameDescriptor
{
    public int MinigameTypeId => 1000;
    public string GameKey => "MyGame";
    public string DisplayName => "My Game";
    public bool IsPremium => false;
    public Type MoveType => typeof(MyMove);
    public string MoveDiscriminator => "myGame";

    public void RegisterServices(IServiceCollection services) { /* ... */ }

    public Task<IGameEngine> CreateGameEngineAsync(
        Guid gameId, ImmutableArray<string> playerIds,
        Dictionary<string, object> parameters, IServiceProvider sp)
        => throw new NotImplementedException();

    public Task<IGameEngine?> LoadGameEngineAsync(Guid gameId, IServiceProvider sp)
        => throw new NotImplementedException();

    public Task SaveGameStateAsync(Guid gameId, IGameState state, IServiceProvider sp)
        => throw new NotImplementedException();
}
```

You'll also implement `IMinigameFrontendDescriptor` (CSS/JS URLs, lobby route) and an `IGameLogic<TState, TMove>` with the actual game rules.

## Key contracts

| Interface | Purpose |
|---|---|
| `IMinigameDescriptor` | Backend descriptor — lifecycle hooks + DI registration |
| `IMinigameFrontendDescriptor` | Frontend metadata — CSS/JS URLs, lobby route, display info |
| `IGameLogic<TState, TMove>` | Pure game rules (state factory + validator + rules) |
| `IGameEngine` | Runtime instance the host wraps around your logic |
| `IGameStatePersistence` | Host-provided abstraction for persisting state JSON |
| `GameMove`, `GameRequest`, `MoveResult` | Wire types for moves and validation |

## Publishing the plugin

Plugins are loaded at runtime from a directory configured by the host (typically `plugins/<gameKey>/`). Use `dotnet publish` to produce the layout:

```bash
dotnet publish -c Release -o /path/to/plugins/MyGame
```

The output folder should contain your DLL, its `.deps.json`, optionally a `plugin.json` manifest declaring the entry assembly, and a `wwwroot/` directory with static assets.

## Notes

- Third-party `MinigameTypeId` values must be `≥ 1000` (1–999 are reserved for first-party plugins).
- The host's `PluginLoadContext` redirects `Quizanchos.Core` / `Quizanchos.Common` / `Microsoft.*` / `System.*` to the default load context, so descriptor types are reference-equal across the plugin boundary.
- Game state is persisted as a JSON blob via `IGameStatePersistence` — your plugin doesn't need its own database.

## License

MIT
