namespace Quizanchos.WebApi.Dto;

public record QuizCardFloatWithAnswerDto(
    Guid Id,
    int CardIndex,
    int? OptionPicked,
    DateTime CreationTime,
    Guid[] EntitiesId,
    int CorrectOption,
    float[] OptionValues
) : QuizCardFloatDto(Id, CardIndex, OptionPicked, CreationTime, EntitiesId);
