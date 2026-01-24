namespace Quizanchos.Quiz.Dto.Abstractions;

public abstract record QuizCardDtoAbstract(
    Guid Id,
    int CardIndex,
    int? OptionPicked,
    DateTime CreationTime,
    Guid[] EntitiesId
);
