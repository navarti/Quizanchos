using Quizanchos.WebApi.Dto.Abstractions;

namespace Quizanchos.WebApi.Dto;

public record QuizCardFloatDto(
    Guid Id,
    int CardIndex,
    int? OptionPicked,
    DateTime CreationTime,
    Guid[] EntitiesId
) : QuizCardDtoAbstract(Id, CardIndex, OptionPicked, CreationTime, EntitiesId);
