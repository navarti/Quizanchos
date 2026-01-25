using Quizanchos.Common.Enums;
using Quizanchos.Core;
using System.Collections.Immutable;

namespace Quizanchos.WebApi.Services;

public interface IGameLogicFactory
{
    object CreateGameEngine(MinigameType type, Guid gameId, ImmutableArray<Guid> playerIds, Dictionary<string, object> parameters);
    Type GetStateType(MinigameType type);
    Type GetMoveType(MinigameType type);
}
