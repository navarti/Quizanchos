# How to Add a New Minigame (Plugin-Driven)

This project uses a plugin-driven minigame architecture. New minigames are discovered through descriptors and registries, so core runtime flow stays generic.

This guide follows repository rules from `../.github/copilot-instructions.md`:
- keep minigames independent from platform code,
- keep backend and frontend metadata consistent,
- use Domain repositories for CRUD,
- avoid hardcoded minigame-specific logic in common platform layers.

## 1) Create a new minigame project

Create a new class library project targeting `.NET 10` (same as existing minigames), for example:
- `Minigames/YourGame/Quizanchos.YourGame.csproj`

Add references similar to existing minigames:
- `Quizanchos.Core`
- `Quizanchos.Domain`
- `Quizanchos.Common` (if needed)
- `Microsoft.Extensions.DependencyInjection.Abstractions`

## 2) Implement game logic types

In your minigame project, add:
- move type inheriting `GameMove` (example: `YourGameMove`)
- state type implementing/deriving from current game-state pattern (example: `YourGameState`)
- game logic/engine implementation
- engine factory and state service for create/load/save

Keep minigame-specific behavior inside this minigame project.

## 3) Register minigame services via extension methods

Create an extension class, e.g. `Extensions/YourGameServiceExtensions.cs`, and register only your game services there.

If your game needs persistence, use Domain repositories (do not access EF entities directly from controllers).

## 4) Add backend descriptor (`IMinigameDescriptor`)

Create `Descriptors/YourGameMinigameDescriptor.cs` implementing `IMinigameDescriptor`.

Required fields:
- `MinigameTypeId` (unique integer)
- `GameKey` (unique string route key)
- `DisplayName`
- `MoveType` (your `GameMove` type)
- `MoveDiscriminator` (unique string for polymorphic JSON)

Required behavior:
- `RegisterServices(IServiceCollection)` calls your extension methods.
- `CreateGameEngineAsync(...)` resolves your factory and returns `GameEngineWrapper<TState, TMove>`.
- `LoadGameEngineAsync(...)` loads and wraps existing engine.
- `SaveGameStateAsync(...)` saves your concrete state.

## 5) Add frontend descriptor (`IMinigameFrontendDescriptor`)

Create `Descriptors/YourGameFrontendDescriptor.cs` implementing `IMinigameFrontendDescriptor`.

Use the same identity values as backend descriptor:
- same `MinigameTypeId`
- same `GameKey`

Provide frontend metadata:
- card data (`DisplayName`, `Description`, `CardStyle`, `ActionText`, `Order`)
- routes (`LobbyUrl`, `GameUrlTemplate`) using generic host routes:
  - `/Minigame/{gameKey}`
  - `/Minigame/{gameKey}/{gameId}`
- static assets (`LobbyStyles`, `LobbyScripts`, `GameStyles`, `GameScripts`)
- host mode (`LobbyViewType`, `GameViewType`) consistent with existing descriptors

## 6) Add plugin to WebApi build/publish pipeline

`Startup.LoadPluginAssemblies()` scans loaded `Quizanchos.*` assemblies, and `CreateDescriptors<T>()` discovers descriptor implementations.

To ensure your plugin DLL is available in output/publish, update `Quizanchos.WebApi/Quizanchos.WebApi.csproj`:

- Add your project to `MinigamePluginProject` item group.

Example pattern (already used for existing plugins):
- `BuildAndCopyMinigamePlugins` target copies plugin DLL to output directory.
- `PublishMinigamePlugins` target adds plugin DLLs to publish artifacts.

Without this step, descriptor discovery may not find your plugin at runtime.

## 7) Add static frontend assets

Place files under `Quizanchos.WebApi/wwwroot/minigames/your-game/...` and reference those paths in `YourGameFrontendDescriptor`.

The generic host views (`MinigameViewController`) will include these files based on descriptor metadata.

## 8) Verify integration

After build/startup:
- your backend descriptor should be present in `IMinigameRegistry`
- your frontend descriptor should be present in `IMinigameFrontendRegistry`
- your minigame card should appear via `HomeController` (`GetMinigameCards`)
- routes should work:
  - `/Minigame/{GameKey}`
  - `/Minigame/{GameKey}/{gameId}`
- game creation should work through existing API using your `MinigameTypeId`

## 9) Uniqueness and consistency checklist

Before committing, confirm:
- `MinigameTypeId` is unique across all games.
- `GameKey` is unique across all games.
- `MoveDiscriminator` is unique across all games.
- Backend and frontend descriptors use the same `MinigameTypeId` and `GameKey`.
- No platform code contains hardcoded behavior for your specific minigame.

## 10) Minimal skeleton

Files typically required:
- `Descriptors/YourGameMinigameDescriptor.cs`
- `Descriptors/YourGameFrontendDescriptor.cs`
- `Extensions/YourGameServiceExtensions.cs`
- `GameLogic/YourGameMove.cs`
- `GameLogic/YourGameState.cs`
- `GameLogic/YourGameLogic.cs`
- `Services/YourGameEngineFactory.cs`
- `Services/YourGameStateService.cs`

---

If you follow this structure, a new minigame is integrated as a plugin without modifying core game flow in `GameService`, `GameLogicFactory`, or controllers.