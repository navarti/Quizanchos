using System.Collections.Concurrent;
using Quizanchos.Common.Enums;
using Quizanchos.Core;

namespace Quizanchos.WebApi.Services;

public class GameEngineManager
{
    private readonly ConcurrentDictionary<(Guid gameId, MinigameType type), object> _gameEngines = new();

    public GameEngine<TState, TMove>? GetEngine<TState, TMove>(Guid gameId, MinigameType type)
        where TState : IGameState
    {
        if (_gameEngines.TryGetValue((gameId, type), out var engine))
        {
            return engine as GameEngine<TState, TMove>;
        }
        return null;
    }

    public void RegisterEngine<TState, TMove>(Guid gameId, MinigameType type, GameEngine<TState, TMove> engine)
        where TState : IGameState
    {
        _gameEngines[(gameId, type)] = engine;
    }

    public bool RemoveEngine(Guid gameId, MinigameType type)
    {
        return _gameEngines.TryRemove((gameId, type), out _);
    }

    public bool HasEngine(Guid gameId, MinigameType type)
    {
        return _gameEngines.ContainsKey((gameId, type));
    }
}
