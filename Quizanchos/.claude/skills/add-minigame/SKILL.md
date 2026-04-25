---
name: add-minigame
description: Scaffold a new minigame plugin for the Quizanchos platform. Use when the user wants to add, create, or scaffold a new minigame/game plugin (e.g. "add a Minesweeper minigame", "scaffold a new game plugin", "create a new minigame called X"). Generates the project structure, descriptors, game logic, services, frontend asset stubs, registers the plugin in WebApi.csproj and the solution.
---

# Add New Minigame Plugin

You are adding a new minigame plugin to the Quizanchos platform. Ask the user for the following details (if not already provided):

1. **Game name** (e.g., "Minesweeper") — used for class prefixes and display
2. **Game key** (e.g., "Minesweeper") — unique identifier, used in URLs and registry
3. **MinigameTypeId** — next available integer (check existing descriptors to find the highest current ID and increment by 1)
4. **Is premium?** (default: false)
5. **Is multiplayer?** (default: false)
6. **Brief description** of the game rules/mechanics — needed to implement the game logic
7. **Move structure** — what data a player sends per move (e.g., "row and column for a cell click")
8. **Project location** — either top-level (`Quizanchos.{GameKey}/`) or under `Minigames/{GameKey}/Quizanchos.{GameKey}/` (default: top-level for simple games, `Minigames/` for complex ones)

## Steps to Execute

### 1. Determine the next MinigameTypeId

Search existing descriptors for the highest `MinigameTypeId` and use the next integer.

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
    {GameKey}StateService.cs
  Extensions/
    {GameKey}ServiceExtensions.cs
  Quizanchos.{GameKey}.csproj
```

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
    <ProjectReference Include="..\Quizanchos.Domain\Quizanchos.Domain.csproj" />
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

#### 3.5 — `{GameKey}StateService.cs`

```csharp
using System.Text.Json;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.{GameKey}.GameLogic;

namespace Quizanchos.{GameKey}.Services;

public class {GameKey}StateService
{
    private readonly IGameSessionStateRepository _repository;

    public {GameKey}StateService(IGameSessionStateRepository repository)
    {
        _repository = repository;
    }

    public async Task<{GameKey}State?> LoadStateAsync(Guid gameSessionId)
    {
        GameSessionState? sessionState = await _repository.GetByGameSessionIdAsync(gameSessionId);
        if (sessionState == null)
            return null;

        {GameKey}State? state = JsonSerializer.Deserialize<{GameKey}State>(sessionState.StateJson);
        if (state == null)
            return null;

        state.GameId = sessionState.GameSessionId;
        state.Players = sessionState.GameSession.Players.Select(p => p.ApplicationUserId).ToList();
        state.IsFinished = sessionState.GameSession.IsFinished;
        state.Winner = sessionState.GameSession.WinnerId;

        return state;
    }

    public async Task SaveStateAsync(Guid gameSessionId, {GameKey}State state)
    {
        GameSessionState? existingState = await _repository.GetByGameSessionIdAsync(gameSessionId);
        if (existingState == null)
        {
            throw new InvalidOperationException($"GameSessionState not found for GameSessionId: {gameSessionId}");
        }

        existingState.StateJson = JsonSerializer.Serialize(state);
        existingState.UpdatedAt = DateTime.UtcNow;

        existingState.GameSession.IsFinished = state.IsFinished;
        if (!string.IsNullOrEmpty(state.Winner))
        {
            existingState.GameSession.WinnerId = state.Winner;
            existingState.GameSession.FinishedAt = DateTime.UtcNow;
        }
        if (state.IsFinished)
        {
            existingState.GameSession.IsActive = false;
        }

        await _repository.UpdateAsync(existingState);
    }

    public async Task<GameSessionState> CreateInitialStateAsync(
        GameSession gameSession,
        {GameKey}State state)
    {
        var sessionState = new GameSessionState
        {
            Id = Guid.NewGuid(),
            GameSessionId = gameSession.Id,
            MinigameType = gameSession.MinigameType,
            StateJson = JsonSerializer.Serialize(state),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.CreateAsync(sessionState);
        return sessionState;
    }
}
```

#### 3.6 — `{GameKey}EngineFactory.cs`

```csharp
using Microsoft.Extensions.Logging;
using Quizanchos.Core;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.{GameKey}.GameLogic;
using System.Collections.Immutable;

namespace Quizanchos.{GameKey}.Services;

public class {GameKey}EngineFactory
{
    private const int {GameKey}MinigameTypeId = {MinigameTypeId};
    private readonly ILogger<{GameKey}EngineFactory> _logger;
    private readonly {GameKey}StateService _stateService;
    private readonly IGameSessionRepository _gameSessionRepository;

    public {GameKey}EngineFactory(
        ILogger<{GameKey}EngineFactory> logger,
        {GameKey}StateService stateService,
        IGameSessionRepository gameSessionRepository)
    {
        _logger = logger;
        _stateService = stateService;
        _gameSessionRepository = gameSessionRepository;
    }

    public async Task<GameEngine<{GameKey}State, {GameKey}Move>> CreateEngineAsync(
        Guid gameId,
        ImmutableArray<string> playerIds)
    {
        _logger.LogInformation("Creating {GameKey} engine");

        GameSession gameSession = new GameSession
        {
            Id = gameId,
            MinigameType = {GameKey}MinigameTypeId,
            IsActive = true,
            IsFinished = false,
            CreatedAt = DateTime.UtcNow
        };

        foreach (string playerId in playerIds)
        {
            gameSession.Players.Add(new GameSessionPlayer
            {
                Id = Guid.NewGuid(),
                GameSessionId = gameId,
                ApplicationUserId = playerId,
                JoinedAt = DateTime.UtcNow
            });
        }

        await _gameSessionRepository.CreateAsync(gameSession);

        // TODO: Pass any game-specific parameters to the logic constructor
        {GameKey}Logic logic = new {GameKey}Logic();
        GameEngine<{GameKey}State, {GameKey}Move> engine = new(logic, gameId, playerIds);

        await _stateService.CreateInitialStateAsync(gameSession, engine.State);

        return engine;
    }

    public async Task<GameEngine<{GameKey}State, {GameKey}Move>?> LoadEngineAsync(Guid gameId)
    {
        _logger.LogInformation("Loading {GameKey} engine for GameId={GameId}", gameId);

        {GameKey}State? state = await _stateService.LoadStateAsync(gameId);
        if (state == null)
        {
            _logger.LogWarning("{GameKey} state not found for GameId={GameId}", gameId);
            return null;
        }

        // TODO: Reconstruct logic with the same parameters used at creation
        {GameKey}Logic logic = new {GameKey}Logic();
        return new GameEngine<{GameKey}State, {GameKey}Move>(logic, state);
    }

    public async Task SaveStateAsync(Guid gameId, {GameKey}State state)
    {
        await _stateService.SaveStateAsync(gameId, state);
    }
}
```

#### 3.7 — `{GameKey}ServiceExtensions.cs`

```csharp
using Microsoft.Extensions.DependencyInjection;

namespace Quizanchos.{GameKey}.Extensions;

public static class {GameKey}ServiceExtensions
{
    public static IServiceCollection Add{GameKey}Services(this IServiceCollection services)
    {
        services.AddScoped<Services.{GameKey}StateService>();
        services.AddScoped<Services.{GameKey}EngineFactory>();
        return services;
    }
}
```

#### 3.8 — `{GameKey}MinigameDescriptor.cs` (implements `IMinigameDescriptor`)

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

#### 3.9 — `{GameKey}FrontendDescriptor.cs` (implements `IMinigameFrontendDescriptor`)

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
- [ ] `MinigameTypeId` is unique across all minigames
- [ ] Plugin is registered in `WebApi.csproj` as `<MinigamePluginProject>`
- [ ] Project is added to `Quizanchos.slnx`
- [ ] Frontend asset stubs exist in `wwwroot/minigames/{gameKeyLower}/`
- [ ] Plugin only references `Core`, `Common`, and `Domain` — NOT `WebApi`
