namespace Quizanchos.Core;

public interface IGameRules<TState, TMove>
    where TState : IGameState
{
    void ApplyMove(TState state, TMove move, string playerId);
    bool CheckFinished(TState state);
    string? DetermineWinner(TState state);
    IEnumerable<string> GetExpectedPlayers(TState state);
    // ??? TicTacToe -> ???? ???????
    // ??? Quiz -> ??? ??????
}
