namespace Quizanchos.WebApi.Dto;

public record QuizCardIntWithAnswerDto(
    Guid Id,
    int CardIndex,
    int? OptionPicked,
    DateTime CreationTime,
    Guid Entity1Id,
    Guid Entity2Id,
    int CorrectOption,
    int Option1Value,
    int Option2Value
) : QuizCardIntDto(Id, CardIndex, OptionPicked, CreationTime, Entity1Id, Entity2Id);
