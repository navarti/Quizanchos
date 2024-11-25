namespace Quizanchos.WebApi.Dto;

public record QuizCardFloatWithAnswerDto(
    Guid Id,
    int CardIndex,
    int? OptionPicked,
    DateTime CreationTime,
    Guid Entity1Id,
    Guid Entity2Id,
    int CorrectOption,
    float Option1Value,
    float Option2Value
) : QuizCardFloatDto(Id, CardIndex, OptionPicked, CreationTime, Entity1Id, Entity2Id);
