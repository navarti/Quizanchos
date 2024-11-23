using Quizanchos.Common.Enums;
using Quizanchos.Domain.Entities.Interfaces;

namespace Quizanchos.Domain.Entities;

public class SingleGameSession : IKeyedEntity<Guid>
{
    public Guid Id { get; set; }
    public DateTime CreationTime { get; set; }
    public int CardsCount { get; set; }
    public int CurrentCardIndex { get; set; }
    public int Score { get; set; }
    public bool IsFinished { get; set; }
    public bool IsTerminatedByTime { get; set; }
    public GameLevel GameLevel { get; set; }
    public int SecondsPerCard { get; set; }

    public ApplicationUser ApplicationUser { get; set; }
    public QuizCategory QuizCategory { get; set; }
}
