# Minigame Plugin Architecture Implementation

## Overview

The platform now uses a **plugin-based architecture** for minigame integration. This allows adding new minigames **without modifying WebApi startup registration** and without changing any shared enum.

## Architecture Components

### 1. Core Interfaces (in `Quizanchos.Core`)

#### `IMinigameDescriptor`
- Defines the contract for minigame plugins
- Each minigame must implement this interface
- Responsible for:
  - Self-registering services (`RegisterServices`)
  - Creating game engine instances (`CreateGameEngineAsync`)
  - Loading saved games (`LoadGameEngineAsync`)
  - Persisting game state (`SaveGameStateAsync`)

#### `IMinigameRegistry`
- Manages registration and retrieval of minigame descriptors
- Thread-safe singleton that holds all registered minigames
- Methods:
  - `Register()` - Register a new minigame descriptor
  - `GetDescriptor()` - Retrieve a descriptor by key
  - `GetAllDescriptors()` - Get all registered minigames
  - `IsRegistered()` - Check if a minigame is registered

#### `IGameEngine`
- (Moved from WebApi to Core)
- Generic interface that all game engines implement
- Provides unified API for game operations

#### `GameEngineWrapper<TState, TMove>`
- (Moved from WebApi to Core)
- Generic adapter that wraps strongly-typed game engines
- Adapts specific `GameEngine<TState, TMove>` implementations to `IGameEngine` interface

### 2. Registry Implementation (in `Quizanchos.WebApi`)

#### `MinigameRegistry`
- Concrete implementation of `IMinigameRegistry`
- Thread-safe dictionary-based storage
- Validates descriptor keys to prevent duplicates

### 3. Minigame Descriptors (in each minigame project)

Each minigame project now has a `Descriptors` folder with a descriptor class:

#### `Quizanchos.Quiz/Descriptors/QuizMinigameDescriptor.cs`
```csharp
public class QuizMinigameDescriptor : IMinigameDescriptor
{
    public int MinigameTypeId => 1;
    public string GameKey => "Quiz";
    public string DisplayName => "Quiz";
    
    // Calls Quiz extension methods to register services
    public void RegisterServices(IServiceCollection services)
    
    // Creates new Quiz game engine instances
    public async Task<IGameEngine> CreateGameEngineAsync(...)
    
    // Loads persisted Quiz games
    public async Task<IGameEngine?> LoadGameEngineAsync(...)
    
    // Saves Quiz game state
    public async Task SaveGameStateAsync(...)
}
```

Similar descriptors exist for:
- `Quizanchos.Game2048/Descriptors/Game2048MinigameDescriptor.cs`
- `Quizanchos.QuizMultiplayer/Descriptors/QuizMultiplayerMinigameDescriptor.cs`

## How It Works

### Startup Flow

1. **Initialization** (in `Startup.cs`):
   ```csharp
   var pluginAssemblies = LoadPluginAssemblies();
   var registry = BuildMinigameRegistry(services, pluginAssemblies);
   services.AddSingleton<IMinigameRegistry>(registry);

   var frontendRegistry = BuildFrontendRegistry(pluginAssemblies);
   services.AddSingleton<IMinigameFrontendRegistry>(frontendRegistry);
   ```

2. **Factory Usage** (in `GameLogicFactory`):
   ```csharp
   public async Task<IGameEngine> CreateGameEngine(
       int type, Guid gameId, ...)
   {
       var descriptor = _registry.GetDescriptor(type);
       if (descriptor == null)
           throw new ArgumentException($"Unknown minigame type: {type}");
       
       // Delegate to descriptor
       return await descriptor.CreateGameEngineAsync(
           gameId, playerIds, parameters, _serviceProvider);
   }
   ```

### Game Creation Flow

```
GameController.CreateGame
    ↓
GameService.CreateGameAsync
    ↓
GameLogicFactory.CreateGameEngine(1)
    ↓
IMinigameRegistry.GetDescriptor(1)
    ↓
QuizMinigameDescriptor.CreateGameEngineAsync
    ↓
QuizEngineFactory.CreateQuizEngineAsync
    ↓
GameEngineWrapper<QuizGameState, QuizMove>
```

## Adding a New Minigame

### Step 1: Create Your Minigame Project
```
Minigames/YourGame/Quizanchos.YourGame/
```

### Step 2: Create a Descriptor
File: `Descriptors/YourGameMinigameDescriptor.cs`

```csharp
using Microsoft.Extensions.DependencyInjection;
using Quizanchos.Core;
using System.Collections.Immutable;

namespace Quizanchos.YourGame.Descriptors;

public class YourGameMinigameDescriptor : IMinigameDescriptor
{
    public int MinigameTypeId => 4;
    public string GameKey => "YourGame";
    public string DisplayName => "Your Game Name";

    public void RegisterServices(IServiceCollection services)
    {
        // Register repositories and services
        services.AddYourGameRepositories();
        services.AddYourGameServices();
    }

    public async Task<IGameEngine> CreateGameEngineAsync(
        Guid gameId, 
        ImmutableArray<string> playerIds,
        Dictionary<string, object> parameters, 
        IServiceProvider serviceProvider)
    {
        var factory = serviceProvider.GetRequiredService<YourGameEngineFactory>();
        
        // Extract parameters
        var customParam = parameters.ContainsKey("customParam")
            ? (string)parameters["customParam"]
            : "default";
        
        // Create engine
        var engine = await factory.CreateYourGameEngineAsync(
            gameId, playerIds, customParam);
        
        // Wrap and return
        return new GameEngineWrapper<YourGameState, YourGameMove>(engine);
    }

    public async Task<IGameEngine?> LoadGameEngineAsync(
        Guid gameId, 
        IServiceProvider serviceProvider)
    {
        var factory = serviceProvider.GetRequiredService<YourGameEngineFactory>();
        var engine = await factory.LoadYourGameEngineAsync(gameId);
        
        return engine != null 
            ? new GameEngineWrapper<YourGameState, YourGameMove>(engine)
            : null;
    }

    public async Task SaveGameStateAsync(
        Guid gameId, 
        IGameState state, 
        IServiceProvider serviceProvider)
    {
        var factory = serviceProvider.GetRequiredService<YourGameEngineFactory>();
        
        if (state is YourGameState yourGameState)
        {
            await factory.SaveYourGameStateAsync(gameId, yourGameState);
        }
    }
}
```

### Step 3: Create Service Extension
File: `Extensions/YourGameServiceExtensions.cs`

```csharp
using Microsoft.Extensions.DependencyInjection;

namespace Quizanchos.YourGame.Extensions;

public static class YourGameServiceExtensions
{
    public static IServiceCollection AddYourGameRepositories(
        this IServiceCollection services)
    {
        // Register repositories
        return services;
    }

    public static IServiceCollection AddYourGameServices(
        this IServiceCollection services)
    {
        // Register services
        services.AddScoped<YourGameStateService>();
        services.AddScoped<YourGameEngineFactory>();
        return services;
    }
}
```

### Step 4: Assign a Numeric Type ID in Descriptor

```csharp
public class YourGameMinigameDescriptor : IMinigameDescriptor
{
    public int MinigameTypeId => 4;
    public string GameKey => "YourGame";
    // ...
}
```

### Step 5: Add frontend descriptor with the same `MinigameTypeId`

```csharp
public class YourGameFrontendDescriptor : IMinigameFrontendDescriptor
{
    public int MinigameTypeId => 4;
    public string GameKey => "YourGame";
    // ...
}
```

**Important:** `MinigameTypeId` must be unique and match between backend and frontend descriptors.

### That's It! ✅

No other WebApi code needs modification. Your minigame is now fully integrated and available through the standard game creation API.

## Benefits of This Architecture

| Benefit | Details |
|---------|---------|
| **No WebApi Modification** | Add new games without touching WebApi core code |
| **Self-Contained** | Each minigame handles its own setup |
| **Scalable** | Add unlimited minigames using the same pattern |
| **Type-Safe** | Strong typing maintained through generics |
| **Testable** | Mock descriptors easily for unit tests |
| **SOLID Compliant** | Follows Open/Closed Principle |
| **Decoupled from shared enums** | New games do not require editing common enum definitions |

## Key Design Decisions

### Why `MinigameTypeId` and `GameKey`?
`MinigameTypeId` is the runtime/persistence identity used by APIs and registries. `GameKey` is a stable human-readable key for UI/routing metadata.

```csharp
// In GameLogicFactory
var descriptor = _registry.GetDescriptor(minigameTypeId);
```

### Why Services Are Registered in Descriptors?
This allows each minigame to control its own dependency injection setup. Services aren't registered until the descriptor registers them, reducing namespace pollution and enabling lazy initialization if needed.

### Why `GameEngineWrapper`?
Different minigames have different state and move types (`QuizGameState`/`QuizMove`, `Game2048State`/`Game2048Move`, etc.). The wrapper adapts these specific types to the generic `IGameEngine` interface, allowing the factory to work with any minigame uniformly.

## Migration Notes

- The `GameEngineWrapper` class was moved from `WebApi.Services.GameLogic` to `Quizanchos.Core` to make it accessible to all minigame projects.
- The `IGameEngine` interface was moved from `WebApi.Services.GameLogic` to `Quizanchos.Core` for the same reason.
- Old imports in `Quizanchos.WebApi` are preserved with a note for backward compatibility.

## Troubleshooting

### Issue: "Unknown minigame type" error at runtime
**Cause:** Descriptor was not discovered/registered or `MinigameTypeId` is wrong
**Solution:** 
- Verify descriptor class is public and has a parameterless constructor
- Ensure `MinigameTypeId` is unique and greater than zero
- Ensure your minigame project is referenced by WebApi wildcard project references and built

### Issue: Services not being injected into game engine factory
**Cause:** `RegisterServices` wasn't called on descriptor
**Solution:**
- Verify descriptor is discovered during startup
- Ensure the method calls the appropriate extension methods (e.g., `AddYourGameRepositories()`)

### Issue: Game creation fails with serialization errors
**Cause:** Parameters passed to descriptor have wrong types
**Solution:**
- In `CreateGameEngineAsync`, properly extract and convert parameters
- Use `ContainsKey` to check before accessing parameters
- Provide sensible defaults for missing parameters

## Testing

To test your new minigame integration:

```csharp
[Fact]
public void QuizMinigameDescriptor_ShouldRegisterWithCorrectKey()
{
    var descriptor = new QuizMinigameDescriptor();
    Assert.Equal(nameof(MinigameType.Quiz), descriptor.GameKey);
}

[Fact]
public void MinigameRegistry_ShouldRetrieveRegisteredDescriptor()
{
    var registry = new MinigameRegistry();
    var descriptor = new QuizMinigameDescriptor();
    
    registry.Register(descriptor);
    
    var retrieved = registry.GetDescriptor("Quiz");
    Assert.NotNull(retrieved);
    Assert.Equal(descriptor.GameKey, retrieved.GameKey);
}
```

## Future Enhancements

1. **Dynamic Loading:** Load minigames from separate assemblies at runtime
2. **Discovery Service:** Automatically discover all `IMinigameDescriptor` implementations
3. **Configuration-Based Registry:** Load minigame definitions from `appsettings.json`
4. **Hot Reload:** Support reloading minigames without restarting the application
