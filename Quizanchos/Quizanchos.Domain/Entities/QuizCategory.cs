using Quizanchos.Common.Enums;
using Quizanchos.Domain.Entities.Interfaces;

namespace Quizanchos.Domain.Entities;

public class QuizCategory : IKeyedEntity<Guid>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public FeatureType FeatureType { get; set; }
}
