using Quizanchos.Common.Enums;
using Quizanchos.Core;

namespace Quizanchos.Quiz.GameLogic;

public class QuizGameState : IGameState
{
    public MinigameType MinigameType => MinigameType.Quiz;

    public Guid GameId { get; set; }
    public IReadOnlyList<Guid> Players { get; set; } = Array.Empty<Guid>();
    public bool IsFinished { get; set; }
    public Guid? Winner { get; set; }

    // Quiz-specific state
    public int CurrentCardIndex { get; set; } = -1;
    public Dictionary<Guid, int> PlayerScores { get; set; } = new();
    public List<QuizCard> Cards { get; set; } = new();
    public int TotalCards { get; set; }
    
    // Quiz configuration
    public Guid QuizCategoryId { get; set; }
    public GameLevel GameLevel { get; set; }
    public int SecondsPerCard { get; set; }
    public int OptionCount { get; set; }
    public DateTime CreationTime { get; set; }
    public bool IsTerminatedByTime { get; set; }

    public class QuizCard
    {
        public Guid Id { get; set; }
        public int CardIndex { get; set; }
        public int CorrectOption { get; set; }
        public Dictionary<Guid, int?> PlayerAnswers { get; set; } = new();
        public Guid[] EntityIds { get; set; } = Array.Empty<Guid>();
        public DateTime CreationTime { get; set; }
        public int? OptionPicked { get; set; }
        
        // Full entity data for display
        public string[] EntityNames { get; set; } = Array.Empty<string>();
        public object[] OptionValues { get; set; } = Array.Empty<object>();
    }

    public QuizGameState()
    {

    }
}
