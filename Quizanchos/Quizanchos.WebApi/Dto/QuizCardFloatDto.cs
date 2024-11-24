using Quizanchos.WebApi.Dto.Abstractions;

namespace Quizanchos.WebApi.Dto;

public record QuizCardFloatDto(
    Guid Id,
    int CardIndex,
    int? OptionPicked,
    DateTime CreationTime,
    Guid Entity1Id,
    Guid Entity2Id
) : QuizCardDtoAbstract(Id, CardIndex, OptionPicked, CreationTime, Entity1Id, Entity2Id);
