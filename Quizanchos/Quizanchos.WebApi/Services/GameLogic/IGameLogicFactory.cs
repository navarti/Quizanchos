using Quizanchos.Common.Enums;
using Quizanchos.Core;
using System.Collections.Immutable;

namespace Quizanchos.WebApi.Services.GameLogic;

public interface IGameLogicFactory
{
    Task<IGameEngine> CreateGameEngine(MinigameType type, Guid gameId, ImmutableArray<Guid> playerIds, Dictionary<string, object> parameters);
    Task<IGameEngine?> LoadGameEngine(MinigameType type, Guid gameId);
    Task SaveGameState(MinigameType type, Guid gameId, IGameState state);
}
