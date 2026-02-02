using Quizanchos.Common.Enums;
using Quizanchos.Domain.Entities.Interfaces;

namespace Quizanchos.Domain.Entities.Quiz;

public class QuizGameSessionState : IKeyedEntity<Guid>
{
    public Guid Id { get; set; }
    public Guid GameSessionId { get; set; }
    
    // Quiz configuration
    public Guid QuizCategoryId { get; set; }
    public GameLevel GameLevel { get; set; }
    public int SecondsPerCard { get; set; }
    public int OptionCount { get; set; }
    public int TotalCards { get; set; }
    
    // Quiz state
    public int CurrentCardIndex { get; set; }
    public bool IsTerminatedByTime { get; set; }
    public DateTime CreationTime { get; set; }

    // Navigation properties
    public GameSession GameSession { get; set; } = null!;
    public QuizCategory QuizCategory { get; set; } = null!;
    public ICollection<QuizSessionCard> Cards { get; set; } = new List<QuizSessionCard>();
    public ICollection<QuizSessionPlayerScore> PlayerScores { get; set; } = new List<QuizSessionPlayerScore>();
}
