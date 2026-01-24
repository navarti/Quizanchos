namespace Quizanchos.Quiz.Entities.Interfaces;

public interface IKeyedEntity<TKey>
{
    TKey Id { get; set; }
}
