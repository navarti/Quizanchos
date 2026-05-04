---
name: add-minigame
description: Scaffold a new minigame plugin for the Quizanchos platform. Use when the user wants to add, create, or scaffold a new minigame/game plugin (e.g. "add a Minesweeper minigame", "scaffold a new game plugin", "create a new minigame called X"). Generates the project structure, descriptors, game logic, services, frontend asset stubs, registers the plugin in WebApi.csproj and the solution.
---

# Add New Minigame Plugin

You are adding a new **first-party** minigame plugin to the Quizanchos platform — i.e. one that lives inside this repo and is bundled at build time via the `<MinigamePluginProject>` MSBuild target. For the third-party (drop-in DLL) workflow, see the sidebar at the bottom of this file.

Ask the user for the following details (if not already provided):

1. **Game name** (e.g., "Minesweeper") — used for class prefixes and display
2. **Game key** (e.g., "Minesweeper") — unique identifier, used in URLs and registry
3. **MinigameTypeId** — next available integer in the first-party range (1–999). Check existing descriptors and use the next free integer.
4. **Is premium?** (default: false)
5. **Is multiplayer?** (default: false)
6. **Brief description** of the game rules/mechanics — needed to implement the game logic
7. **Move structure** — what data a player sends per move (e.g., "row and column for a cell click")
8. **Project location** — either top-level (`Quizanchos.{GameKey}/`) or under `Minigames/{GameKey}/Quizanchos.{GameKey}/` (default: top-level for simple games, `Minigames/` for complex ones)

## Steps to Execute

### 1. Determine the next MinigameTypeId

Search existing descriptors for the highest first-party `MinigameTypeId` (in the 1–999 range; 1000+ is reserved for third-party) and use the next integer.

```bash
# Find all MinigameTypeId values
grep -r "MinigameTypeId =>" --include="*.cs" .
```

### 2. Create the project directory structure

```
Quizanchos.{GameKey}/
  Descriptors/
    {GameKey}MinigameDescriptor.cs
    {GameKey}FrontendDescriptor.cs
  GameLogic/
    {GameKey}State.cs
    {GameKey}Move.cs
    {GameKey}Logic.cs
  Services/
    {GameKey}EngineFactory.cs
  Extensions/
    {GameKey}ServiceExtensions.cs
  Quizanchos.{GameKey}.csproj
```

> **Important:** Plugins reference `Quizanchos.Core` + `Quizanchos.Common` only — never `Quizanchos.Domain` or `Quizanchos.WebApi`. State persistence goes through `IGameStatePersistence` (in `Quizanchos.Core`), implemented by the host. There is no separate `{GameKey}StateService` — its responsibilities are absorbed into the engine factory.

### 3. Create files using these templates

#### 3.1 — `.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="10.0.5" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="10.0.5" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Quizanchos.Common\Quizanchos.Common.csproj" />
    <ProjectReference Include="..\Quizanchos.Core\Quizanchos.Core.csproj" />
  </ItemGroup>
</Project>
```

**Important:** Adjust `ProjectReference` paths based on the actual project location relative to the solution root. If the project is under `Minigames/{GameKey}/`, paths need `..\..\` prefixes.

#### 3.2 — `{GameKey}State.cs` (implements `IGameState`)

```csharp
using Quizanchos.Core;

namespace Quizanchos.{GameKey}.GameLogic;

public class {GameKey}State : IGameState
{
    public int MinigameType => {MinigameTypeId};

    public Guid GameId { get; set; }
    public IReadOnlyList<string> Players { get; set; } = Array.Empty<string>();
    public bool IsFinished { get; set; }
    public string? Winner { get; set; }

    // TODO: Add game-specific state properties here
}
```

#### 3.3 — `{GameKey}Move.cs` (extends `GameMove`)

```csharp
using Quizanchos.Core;
using System.Text.Json.Serialization;

namespace Quizanchos.{GameKey}.GameLogic;

[JsonDerivedType(typeof({GameKey}Move), "{moveDiscriminator}")]
public record {GameKey}Move : GameMove
{
    // TODO: Add move-specific properties with [JsonPropertyName("...")] attributes
}
```

The `{moveDiscriminator}` should be the GameKey in lowercase/camelCase (e.g., "minesweeper"). This is used by the polymorphic JSON deserializer.

#### 3.4 — `{GameKey}Logic.cs` (implements `IGameLogic<TState, TMove>`)

```csharp
using System.Collections.Immutable;
using Quizanchos.Core;

namespace Quizanchos.{GameKey}.GameLogic;

public class {GameKey}Logic : IGameLogic<{GameKey}State, {GameKey}Move>
{
    public {GameKey}State CreateInitialState(Guid gameId, ImmutableArray<string> players)
    {
        return new {GameKey}State
        {
            GameId = gameId,
            Players = players.ToList(),
            IsFinished = false,
            Winner = null,
            // TODO: Initialize game-specific state
        };
    }

    public MoveResult ValidateMove({GameKey}State state, {GameKey}Move move, string playerId)
    {
        // TODO: Validate the move is legal
        return MoveResult.Success;
    }

    public void ApplyMove({GameKey}State state, {GameKey}Move move, string playerId)
    {
        // TODO: Apply the move to the state
    }

    public bool CheckFinished({GameKey}State state)
    {
        // TODO: Return true if the game is over
        return false;
    }

    public string? DetermineWinner({GameKey}State state)
    {
        // TODO: Return the winning player's ID, or null
        return null;
    }

    public IEnumerable<string> GetExpectedPlayers({GameKey}State state)
    {
        return state.Players;
    }

    public bool NeedToFinish({GameKey}State state)
    {
        // Return true if a timeout or external condition requires ending the game
        return false;
    }

    public IReadOnlyDictionary<string, int> GetPlayerScores({GameKey}State state)
    {
        // TODO: Return scores per player
        // Default: award 10 points to the winner
        if (!string.IsNullOrEmpty(state.Winner))
        {
            return new Dictionary<string, int> { { state.Winner, 10 } };
        }
        return new Dictionary<string, int>();
    }
}
```

#### 3.5 — `{GameKey}EngineFactory.cs`

The factory uses `IGameStatePersistence` (host-provided abstraction in `Quizanchos.Core`) to create/load/save state. No `IGameSessionRepository`, no Domain entities — the host handles `GameSession` row creation when `CreateAsync` is called.

```csharp
using Microsoft.Extensions.Logging;
using Quizanchos.Core;
using Quizanchos.{GameKey}.GameLogic;
using System.Collections.Immutable;
using System.Text.Json;

namespace Quizanchos.{GameKey}.Services;

public class {GameKey}EngineFactory
{
    private const int MinigameTypeId = {MinigameTypeId};

    private readonly ILogger<{GameKey}EngineFactory> _logger;
    private readonly IGameStatePersistence _persistence;

    public {GameKey}EngineFactory(
        ILogger<{GameKey}EngineFactory> logger,
        IGameStatePersistence persistence)
    {
        _logger = logger;
        _persistence = persistence;
    }

    public async Task<GameEngine<{GameKey}State, {GameKey}Move>> CreateEngineAsync(
        Guid gameId,
        ImmutableArray<string> playerIds)
    {
        _logger.LogInformation("Creating {GameKey} engine for GameId={GameId}", gameId);

        // TODO: Pass any game-specific parameters to the logic constructor
        var logic = new {GameKey}Logic();
        var engine = new GameEngine<{GameKey}State, {GameKey}Move>(logic, gameId, playerIds);

        await _persistence.CreateAsync(
            gameId,
            MinigameTypeId,
            playerIds,
            JsonSerializer.Serialize(engine.State));

        return engine;
    }

    public async Task<GameEngine<{GameKey}State, {GameKey}Move>?> LoadEngineAsync(Guid gameId)
    {
        _logger.LogInformation("Loading {GameKey} engine for GameId={GameId}", gameId);

        var loaded = await _persistence.LoadAsync(gameId);
        if (loaded is null)
        {
            _logger.LogWarning("{GameKey} state not found for GameId={GameId}", gameId);
            return null;
        }

        var state = JsonSerializer.Deserialize<{GameKey}State>(loaded.StateJson);
        if (state is null)
        {
            _logger.LogWarning("{GameKey} state failed to deserialize for GameId={GameId}", gameId);
            return null;
        }

        // Patch host-tracked metadata onto the deserialized state.
        state.GameId = gameId;
        state.Players = loaded.PlayerIds;
        state.IsFinished = loaded.IsFinished;
        state.Winner = loaded.Winner;

        // TODO: Reconstruct logic with the same parameters used at creation.
        var logic = new {GameKey}Logic();
        return new GameEngine<{GameKey}State, {GameKey}Move>(logic, state);
    }

    public async Task SaveStateAsync(Guid gameId, {GameKey}State state)
    {
        await _persistence.UpdateAsync(gameId, JsonSerializer.Serialize(state));
    }
}
```

#### 3.6 — `{GameKey}ServiceExtensions.cs`

```csharp
using Microsoft.Extensions.DependencyInjection;
using Quizanchos.{GameKey}.Services;

namespace Quizanchos.{GameKey}.Extensions;

public static class {GameKey}ServiceExtensions
{
    public static IServiceCollection Add{GameKey}Services(this IServiceCollection services)
    {
        services.AddScoped<{GameKey}EngineFactory>();
        return services;
    }
}
```

#### 3.7 — `{GameKey}MinigameDescriptor.cs` (implements `IMinigameDescriptor`)

```csharp
using Microsoft.Extensions.DependencyInjection;
using Quizanchos.Core;
using Quizanchos.{GameKey}.Extensions;
using Quizanchos.{GameKey}.GameLogic;
using Quizanchos.{GameKey}.Services;
using System.Collections.Immutable;

namespace Quizanchos.{GameKey}.Descriptors;

public class {GameKey}MinigameDescriptor : IMinigameDescriptor
{
    public int MinigameTypeId => {MinigameTypeId};
    public string GameKey => "{GameKey}";
    public string DisplayName => "{DisplayName}";
    public bool IsPremium => {IsPremium};
    public Type MoveType => typeof({GameKey}Move);
    public string MoveDiscriminator => "{moveDiscriminator}";

    public void RegisterServices(IServiceCollection services)
    {
        services.Add{GameKey}Services();
    }

    public async Task<IGameEngine> CreateGameEngineAsync(Guid gameId, ImmutableArray<string> playerIds,
        Dictionary<string, object> parameters, IServiceProvider serviceProvider)
    {
        var factory = serviceProvider.GetRequiredService<{GameKey}EngineFactory>();

        // TODO: Extract game-specific parameters from the dictionary
        var engine = await factory.CreateEngineAsync(gameId, playerIds);

        return new GameEngineWrapper<{GameKey}State, {GameKey}Move>(engine);
    }

    public async Task<IGameEngine?> LoadGameEngineAsync(Guid gameId, IServiceProvider serviceProvider)
    {
        var factory = serviceProvider.GetRequiredService<{GameKey}EngineFactory>();

        var engine = await factory.LoadEngineAsync(gameId);
        if (engine == null)
            return null;

        return new GameEngineWrapper<{GameKey}State, {GameKey}Move>(engine);
    }

    public async Task SaveGameStateAsync(Guid gameId, IGameState state, IServiceProvider serviceProvider)
    {
        var factory = serviceProvider.GetRequiredService<{GameKey}EngineFactory>();

        if (state is {GameKey}State typedState)
        {
            await factory.SaveStateAsync(gameId, typedState);
        }
    }
}
```

#### 3.8 — `{GameKey}FrontendDescriptor.cs` (implements `IMinigameFrontendDescriptor`)

```csharp
using Quizanchos.Core;

namespace Quizanchos.{GameKey}.Descriptors;

public class {GameKey}FrontendDescriptor : IMinigameFrontendDescriptor
{
    private const string MinigameRoute = "/Minigame/{GameKey}";

    public int MinigameTypeId => {MinigameTypeId};
    public string GameKey => "{GameKey}";
    public string DisplayName => "{DisplayName}";
    public bool IsPremium => {IsPremium};
    public string Description => "{Description}";
    public string CardStyle => "{CardStyle}";
    public string LobbyUrl => MinigameRoute;
    public string GameUrlTemplate => $"{MinigameRoute}/{{gameId}}";
    public string LobbyViewType => "module";
    public string GameViewType => "module";
    public IReadOnlyList<string> LobbyStyles =>
    [
        "/minigames/{gameKeyLower}/css/{gameKeyLower}.css"
    ];
    public IReadOnlyList<string> LobbyScripts =>
    [
        "/js/game-client.js",
        "/minigames/{gameKeyLower}/js/{gameKeyLower}-client.js",
        "/minigames/{gameKeyLower}/js/{gameKeyLower}-lobby.js"
    ];
    public IReadOnlyList<string> GameStyles =>
    [
        "/minigames/{gameKeyLower}/css/{gameKeyLower}.css"
    ];
    public IReadOnlyList<string> GameScripts =>
    [
        "/js/game-client.js",
        "/minigames/{gameKeyLower}/js/{gameKeyLower}-client.js",
        "/minigames/{gameKeyLower}/js/{gameKeyLower}-game.js"
    ];
    public string ActionText => "PLAY";
    public int Order => {MinigameTypeId};
}
```

### 4. Register the plugin in `Quizanchos.WebApi.csproj`

Add a `<MinigamePluginProject>` entry to the existing `<ItemGroup>`:

```xml
<MinigamePluginProject Include="{relative path to .csproj}" />
```

### 5. Add the project to the solution

```bash
dotnet sln Quizanchos.slnx add {path to .csproj}
```

### 6. Create frontend asset stubs

Create the directory structure under `Quizanchos.WebApi/wwwroot/minigames/{gameKeyLower}/`:

```
css/{gameKeyLower}.css        — empty stylesheet
js/{gameKeyLower}-client.js   — shared client utilities (SignalR connection, API calls)
js/{gameKeyLower}-lobby.js    — lobby view logic (start game button, settings)
js/{gameKeyLower}-game.js     — game view logic (render state, handle moves)
```

Create minimal placeholder files so the frontend doesn't 404.

### 7. Implement the actual game logic

Now fill in all the `// TODO` comments with real game logic based on the user's description:
- State properties in `{GameKey}State`
- Move properties in `{GameKey}Move`
- `CreateInitialState`, `ValidateMove`, `ApplyMove`, `CheckFinished`, `DetermineWinner` in `{GameKey}Logic`
- Any game-specific parameters in the engine factory and descriptor
- Frontend JS for rendering and interaction

### 8. Build and verify

```bash
dotnet build Quizanchos.slnx
```

Fix any compilation errors.

## Checklist

Before marking complete, verify:
- [ ] Project compiles with no errors
- [ ] `IMinigameDescriptor` and `IMinigameFrontendDescriptor` are both implemented
- [ ] `IGameLogic<TState, TMove>` is implemented with real game rules
- [ ] Move type has `[JsonDerivedType]` attribute with unique discriminator
- [ ] `MinigameTypeId` is unique and in the first-party range (1–999); third-party uses 1000+
- [ ] Engine factory uses `IGameStatePersistence` — NOT `IGameSessionRepository` or `IGameSessionStateRepository`
- [ ] Plugin is registered in `WebApi.csproj` as `<MinigamePluginProject>`
- [ ] Project is added to `Quizanchos.slnx`
- [ ] Frontend asset stubs exist in `wwwroot/minigames/{gameKeyLower}/`
- [ ] Plugin only references `Core` and `Common` — NOT `WebApi` or `Domain`

---

## Sidebar: Third-party plugin scaffolding

For an externally-developed plugin that drops into the `plugins/` folder at runtime (rather than being bundled at build time), the same plugin code applies — but the surrounding packaging differs:

- **Project lives outside this repo.** The dev develops against the `Quizanchos.Core` SDK (project ref or NuGet, when published).
- **MinigameTypeId must be ≥ 1000** (the loader rejects lower values for third-party plugins).
- **`.csproj` SDK refs use `Private="false" ExcludeAssets="runtime"`** so the host's copy of `Quizanchos.Core`/`Common` is used at runtime (avoids type identity issues and reduces plugin size).
- **A `plugin.json` manifest** at the plugin root declares the entry assembly + wwwroot path.
- **`wwwroot/` lives in the plugin project** (not in `WebApi/wwwroot/`); the host's loader mounts it under `/minigames/{gamekey-lowercase}/` via a per-plugin `PhysicalFileProvider`.
- **Publish via `dotnet publish -c Release -o {pluginsRoot}/{GameKey}`** — drops the DLL, deps.json, manifest, and wwwroot into a single folder ready to be loaded.
- **No edit to `WebApi.csproj` or the solution** — the host discovers the plugin at startup by scanning the configured plugin root (`Plugins:Root` in appsettings).

A complete working example lives at `samples/Quizanchos.Plugin.ClickCounter/` — refer to it as the canonical reference for third-party authoring.
