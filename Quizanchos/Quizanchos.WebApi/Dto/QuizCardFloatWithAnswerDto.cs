namespace Quizanchos.WebApi.Dto;

public record QuizCardFloatWithAnswerDto(
    Guid Id,
    int CardIndex,
    float Option1,
    float Option2,
    int? OptionPicked,
    DateTime CreationTime,
    float CorrectOption
) : QuizCardFloatDto(Id, CardIndex, Option1, Option2, OptionPicked, CreationTime);
