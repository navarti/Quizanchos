using System.Collections.Immutable;

namespace Quizanchos.Core;

public interface IGameStateFactory<TState>
    where TState : IGameState
{
    TState CreateInitialState(Guid gameId, ImmutableArray<Guid> players);
}
