namespace Quizanchos.Core;

public record MoveResult(bool IsSuccess, string Reason)
{
    public static readonly MoveResult Success = new(true, string.Empty);
    public static MoveResult Failure(string reason) => new(false, reason);
}
