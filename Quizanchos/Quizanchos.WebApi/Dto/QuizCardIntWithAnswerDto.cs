namespace Quizanchos.WebApi.Dto;

public record QuizCardIntWithAnswerDto(
    Guid Id,
    int CardIndex,
    int Option1,
    int Option2,
    int? OptionPicked,
    DateTime CreationTime,
    int CorrectOption
) : QuizCardIntDto(Id, CardIndex, Option1, Option2, OptionPicked, CreationTime);
