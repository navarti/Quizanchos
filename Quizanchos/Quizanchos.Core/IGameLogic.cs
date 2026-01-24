namespace Quizanchos.Core;

public interface IGameLogic<TState, TMove> : 
    IGameStateFactory<TState>,
    IGameValidator<TState, TMove>,
    IGameRules<TState, TMove>
    where TState : IGameState
{
}
