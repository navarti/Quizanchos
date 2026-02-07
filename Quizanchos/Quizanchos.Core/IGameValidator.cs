namespace Quizanchos.Core;

public interface IGameValidator<TState, TMove>
    where TState : IGameState
{
    MoveResult ValidateMove(TState state, TMove move, string playerId);
}
