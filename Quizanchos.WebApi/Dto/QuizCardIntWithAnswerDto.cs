namespace Quizanchos.WebApi.Dto;

public record QuizCardIntWithAnswerDto(
    Guid Id,
    int CardIndex,
    int? OptionPicked,
    DateTime CreationTime,
    Guid[] EntitiesId,
    int CorrectOption,
    int[] OptionValues
) : QuizCardIntDto(Id, CardIndex, OptionPicked, CreationTime, EntitiesId);
