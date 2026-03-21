# Quick Reference: Adding a New Minigame

## 5-Minute Setup Guide

### 1️⃣ Create Your Descriptor
**File:** `YourGame/Descriptors/YourGameMinigameDescriptor.cs`

```csharp
using Microsoft.Extensions.DependencyInjection;
using Quizanchos.Core;
using System.Collections.Immutable;

namespace Quizanchos.YourGame.Descriptors;

public class YourGameMinigameDescriptor : IMinigameDescriptor
{
    public string GameKey => "YourGame";
    public string DisplayName => "Your Game Name";

    public void RegisterServices(IServiceCollection services)
    {
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
        var engine = await factory.CreateYourGameEngineAsync(gameId, playerIds);
        return new GameEngineWrapper<YourGameState, YourGameMove>(engine);
    }

    public async Task<IGameEngine?> LoadGameEngineAsync(Guid gameId, IServiceProvider serviceProvider)
    {
        var factory = serviceProvider.GetRequiredService<YourGameEngineFactory>();
        var engine = await factory.LoadYourGameEngineAsync(gameId);
        return engine != null ? new GameEngineWrapper<YourGameState, YourGameMove>(engine) : null;
    }

    public async Task SaveGameStateAsync(Guid gameId, IGameState state, IServiceProvider serviceProvider)
    {
        var factory = serviceProvider.GetRequiredService<YourGameEngineFactory>();
        if (state is YourGameState yourGameState)
            await factory.SaveYourGameStateAsync(gameId, yourGameState);
    }
}
```

### 2️⃣ Register in Startup
**File:** `Quizanchos.WebApi/Startup.cs`

```csharp
var registry = new MinigameRegistry();

// ... existing minigames ...

// ADD YOUR MINIGAME HERE
var yourGameDescriptor = new YourGameMinigameDescriptor();
yourGameDescriptor.RegisterServices(services);
registry.Register(yourGameDescriptor);

services.AddSingleton(registry);
```

### 3️⃣ Add to Enum
**File:** `Quizanchos.Common/Enums/MinigameType.cs`

```csharp
public enum MinigameType
{
    Quiz = 1,
    Game2048 = 2,
    QuizMultiplayer = 3,
    YourGame = 4  // ← ADD HERE
}
```

### ✅ Done!

Your minigame is now fully integrated. No other WebApi modifications needed.

---

## Key Points

| What | Example |
|------|---------|
| **GameKey must match enum name** | `MinigameType.YourGame` → GameKey = `"YourGame"` |
| **Descriptor implements** | `IMinigameDescriptor` |
| **Services registered in** | `RegisterServices()` method |
| **Engine creation via** | Factory pattern (e.g., `YourGameEngineFactory`) |
| **Engine wrapped with** | `GameEngineWrapper<TState, TMove>` |

---

## Common Parameters

```csharp
// Extract parameters from dictionary
var difficulty = parameters.ContainsKey("difficulty") 
    ? (string)parameters["difficulty"] 
    : "normal";

var boardSize = parameters.ContainsKey("boardSize")
    ? (int)parameters["boardSize"]
    : 4;

var categoryId = parameters.ContainsKey("categoryId")
    ? (Guid)parameters["categoryId"]
    : null;
```

---

## Example: Simple Number Game

```csharp
public class NumberGameMinigameDescriptor : IMinigameDescriptor
{
    public string GameKey => "NumberGame";
    public string DisplayName => "Number Game";

    public void RegisterServices(IServiceCollection services)
    {
        services.AddScoped<NumberGameEngineFactory>();
        services.AddScoped<NumberGameStateService>();
    }

    public async Task<IGameEngine> CreateGameEngineAsync(
        Guid gameId, 
        ImmutableArray<string> playerIds,
        Dictionary<string, object> parameters, 
        IServiceProvider serviceProvider)
    {
        var factory = serviceProvider.GetRequiredService<NumberGameEngineFactory>();
        int difficulty = parameters.ContainsKey("difficulty") ? (int)parameters["difficulty"] : 1;
        
        var engine = await factory.CreateEngineAsync(gameId, playerIds, difficulty);
        return new GameEngineWrapper<NumberGameState, NumberGameMove>(engine);
    }

    public async Task<IGameEngine?> LoadGameEngineAsync(Guid gameId, IServiceProvider serviceProvider)
    {
        var factory = serviceProvider.GetRequiredService<NumberGameEngineFactory>();
        var engine = await factory.LoadEngineAsync(gameId);
        return engine != null ? new GameEngineWrapper<NumberGameState, NumberGameMove>(engine) : null;
    }

    public async Task SaveGameStateAsync(Guid gameId, IGameState state, IServiceProvider serviceProvider)
    {
        var factory = serviceProvider.GetRequiredService<NumberGameEngineFactory>();
        if (state is NumberGameState gameState)
            await factory.SaveAsync(gameId, gameState);
    }
}
```

---

## Troubleshooting

| Problem | Solution |
|---------|----------|
| "Unknown minigame type" | Check `GameKey` matches enum name exactly |
| Services not injected | Verify `RegisterServices()` was called in Startup |
| Game won't load | Check factory's `LoadEngineAsync()` returns non-null |
| Serialization errors | Ensure state inherits from `IGameState` |

---

## Testing Your Minigame

```csharp
[Fact]
public void Descriptor_HasCorrectGameKey()
{
    var descriptor = new YourGameMinigameDescriptor();
    Assert.Equal("YourGame", descriptor.GameKey);
}

[Fact]
public async Task CreateEngine_ReturnsValidEngine()
{
    var registry = new MinigameRegistry();
    registry.Register(new YourGameMinigameDescriptor());
    
    var services = new ServiceCollection();
    new YourGameMinigameDescriptor().RegisterServices(services);
    var provider = services.BuildServiceProvider();
    
    var descriptor = registry.GetDescriptor("YourGame");
    var engine = await descriptor.CreateGameEngineAsync(
        Guid.NewGuid(),
        new[] { "player1", "player2" }.ToImmutableArray(),
        new Dictionary<string, object>(),
        provider);
    
    Assert.NotNull(engine);
}
```

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────┐
│                  Your Minigame                      │
│  YourGameMinigameDescriptor : IMinigameDescriptor   │
└─────────────────────────────────────────────────────┘
            ↑
            │ implements
            │
┌─────────────────────────────────────────────────────┐
│            Quizanchos.Core                          │
│  - IMinigameDescriptor                              │
│  - IMinigameRegistry                                │
│  - IGameEngine                                      │
│  - GameEngineWrapper<TState, TMove>                 │
└─────────────────────────────────────────────────────┘
            ↑
            │ uses
            │
┌─────────────────────────────────────────────────────┐
│         Quizanchos.WebApi                           │
│  - MinigameRegistry (singleton)                     │
│  - GameLogicFactory                                 │
│  - Startup.cs (registration)                        │
└─────────────────────────────────────────────────────┘
```

---

## Files to Check

- `MINIGAME_PLUGIN_ARCHITECTURE.md` - Full documentation
- `Minigames/Quiz/Descriptors/QuizMinigameDescriptor.cs` - Example implementation
- `Quizanchos.Core/IMinigameDescriptor.cs` - Interface to implement
- `Quizanchos.WebApi/Startup.cs` - Where to register

---

## That's It! 🎉

You now have everything you need to add a new minigame to the platform without modifying WebApi core code.
