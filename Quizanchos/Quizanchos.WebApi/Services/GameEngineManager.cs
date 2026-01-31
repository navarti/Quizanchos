using System.Collections.Concurrent;
using Quizanchos.Common.Enums;
using Quizanchos.Core;

namespace Quizanchos.WebApi.Services;

public class GameEngineManager
{
    private readonly ConcurrentDictionary<(Guid gameId, MinigameType type), IGameEngine> _gameEngines = new();
    private readonly ConcurrentDictionary<Guid, (Guid gameId, MinigameType type)> _playerToGame = new();

    public IGameEngine? GetEngine(Guid gameId, MinigameType type)
    {
        _gameEngines.TryGetValue((gameId, type), out IGameEngine? engine);
        return engine;
    }

    public IGameEngine? GetEngineByPlayer(Guid playerId)
    {
        if (_playerToGame.TryGetValue(playerId, out (Guid gameId, MinigameType type) gameInfo))
        {
            return GetEngine(gameInfo.gameId, gameInfo.type);
        }
        return null;
    }

    public void RegisterEngine(Guid gameId, MinigameType type, IGameEngine engine)
    {
        _gameEngines[(gameId, type)] = engine;
        
        foreach (Guid playerId in engine.Players)
        {
            _playerToGame[playerId] = (gameId, type);
        }
    }

    public bool RemoveEngine(Guid gameId, MinigameType type)
    {
        if (_gameEngines.TryRemove((gameId, type), out IGameEngine? engine))
        {
            foreach (Guid playerId in engine.Players)
            {
                _playerToGame.TryRemove(playerId, out _);
            }
            return true;
        }
        return false;
    }

    public bool HasEngine(Guid gameId, MinigameType type)
    {
        return _gameEngines.ContainsKey((gameId, type));
    }
}
