using Quizanchos.WebApi.Dto.Abstractions;

namespace Quizanchos.WebApi.Dto;

public record QuizCardIntDto(
    Guid Id,
    int CardIndex,
    int Option1,
    int Option2,
    int OptionPicked
) : QuizCardDtoAbstract(Id, CardIndex, OptionPicked);
