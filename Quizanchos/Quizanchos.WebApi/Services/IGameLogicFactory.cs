using Quizanchos.Common.Enums;
using System.Collections.Immutable;

namespace Quizanchos.WebApi.Services;

public interface IGameLogicFactory
{
    IGameEngine CreateGameEngine(MinigameType type, Guid gameId, ImmutableArray<Guid> playerIds, Dictionary<string, object> parameters);
    Type GetStateType(MinigameType type);
    Type GetMoveType(MinigameType type);
}
