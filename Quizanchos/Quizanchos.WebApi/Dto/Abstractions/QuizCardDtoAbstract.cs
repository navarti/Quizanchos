namespace Quizanchos.WebApi.Dto.Abstractions;

public abstract record QuizCardDtoAbstract(
    Guid Id,
    int CardIndex,
    int? OptionPicked,
    DateTime CreationTime,
    Guid Entity1Id,
    Guid Entity2Id
);