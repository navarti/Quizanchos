using Quizanchos.Core;

namespace Quizanchos.Quiz.GameLogic;

public class QuizGameState : IGameState
{
    public Guid GameId { get; set; }
    public IReadOnlyList<Guid> Players { get; set; } = Array.Empty<Guid>();
    public bool IsFinished { get; set; }
    public Guid? Winner { get; set; }

    // Quiz-specific state
    public int CurrentCardIndex { get; set; } = -1;
    public Dictionary<Guid, int> PlayerScores { get; set; } = new();
    public List<QuizCard> Cards { get; set; } = new();
    public int TotalCards { get; set; }

    public class QuizCard
    {
        public Guid Id { get; set; }
        public int CardIndex { get; set; }
        public int CorrectOption { get; set; }
        public Dictionary<Guid, int?> PlayerAnswers { get; set; } = new();
        public Guid[] EntityIds { get; set; } = Array.Empty<Guid>();
    }
}
