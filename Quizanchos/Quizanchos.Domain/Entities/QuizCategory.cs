using Quizanchos.Common.Enums;
using Quizanchos.Domain.Entities.Interfaces;

namespace Quizanchos.Domain.Entities;

public class QuizCategory : IKeyedEntity<Guid>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public FeatureType FeatureType { get; set; }
    public string ImageUrl { get; set; } = "";
    public string AuthorName { get; set; } = "";
    public DateTime CreationDate { get; set; }
    public string QuestionToDisplay { get; set; } = "";
    public bool IsPremium { get; set; }
}
