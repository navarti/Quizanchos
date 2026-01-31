using System.Collections.Concurrent;
using Quizanchos.Core;

namespace Quizanchos.WebApi.Services;

public class GameEngineManager
{
    private readonly ConcurrentDictionary<Guid, IGameEngine> _gameEngines = new();
    private readonly ConcurrentDictionary<Guid, Guid> _playerToGame = new();

    public IGameEngine? GetEngine(Guid gameId)
    {
        _gameEngines.TryGetValue(gameId, out IGameEngine? engine);
        return engine;
    }

    public IGameEngine? GetEngineByPlayer(Guid playerId)
    {
        if (_playerToGame.TryGetValue(playerId, out Guid gameId))
        {
            return GetEngine(gameId);
        }
        return null;
    }

    public void RegisterEngine(Guid gameId, IGameEngine engine)
    {
        _gameEngines[gameId] = engine;
        
        foreach (Guid playerId in engine.Players)
        {
            _playerToGame[playerId] = gameId;
        }
    }

    public bool RemoveEngine(Guid gameId)
    {
        if (_gameEngines.TryRemove(gameId, out IGameEngine? engine))
        {
            foreach (Guid playerId in engine.Players)
            {
                _playerToGame.TryRemove(playerId, out _);
            }
            return true;
        }
        return false;
    }

    public bool HasEngine(Guid gameId)
    {
        return _gameEngines.ContainsKey(gameId);
    }
}

