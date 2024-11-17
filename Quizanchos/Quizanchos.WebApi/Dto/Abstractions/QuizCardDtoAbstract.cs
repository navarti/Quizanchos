namespace Quizanchos.WebApi.Dto.Abstractions;

public abstract record QuizCardDtoAbstract(
    Guid Id,
    int CardIndex,
    int OptionPicked
);