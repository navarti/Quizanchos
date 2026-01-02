namespace Quizanchos.Core;

public interface IGameRules<TState, TMove>
    where TState : IGameState
{
    void ApplyMove(TState state, TMove move, Guid playerId);
    bool CheckFinished(TState state);
    Guid? DetermineWinner(TState state);
    IEnumerable<Guid> GetExpectedPlayers(TState state);
    // ??? TicTacToe -> ???? ???????
    // ??? Quiz -> ??? ??????
}
