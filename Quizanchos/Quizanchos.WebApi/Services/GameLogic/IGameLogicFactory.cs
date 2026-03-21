using Quizanchos.Core;
using System.Collections.Immutable;

namespace Quizanchos.WebApi.Services.GameLogic;

public interface IGameLogicFactory
{
    Task<IGameEngine> CreateGameEngine(int type, Guid gameId, ImmutableArray<string> playerIds, Dictionary<string, object> parameters);
    Task<IGameEngine?> LoadGameEngine(int type, Guid gameId);
    Task SaveGameState(int type, Guid gameId, IGameState state);
}
