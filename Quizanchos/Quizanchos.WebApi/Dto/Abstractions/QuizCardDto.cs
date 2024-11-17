namespace Quizanchos.WebApi.Dto.Abstractions;

public abstract record QuizCardDto<T>(
    Guid Id,
    int CardIndex,
    T Option1,
    T Option2,
    int OptionPicked
);