using Quizanchos.Domain.Entities.Interfaces;

namespace Quizanchos.Quiz.Entities;

public class QuizSessionCard : IKeyedEntity<Guid>
{
    public Guid Id { get; set; }
    public Guid QuizGameSessionStateId { get; set; }
    
    public int CardIndex { get; set; }
    public int CorrectOption { get; set; }
    public int? OptionPicked { get; set; }
    public DateTime CreationTime { get; set; }
    
    // Stored as JSON arrays
    public string EntityIdsJson { get; set; } = "[]";
    public string EntityNamesJson { get; set; } = "[]";
    public string OptionValuesJson { get; set; } = "[]";

    // Navigation properties
    public QuizGameSessionState QuizGameSessionState { get; set; } = null!;
    public ICollection<QuizSessionCardAnswer> PlayerAnswers { get; set; } = new List<QuizSessionCardAnswer>();
}
