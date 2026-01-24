using Quizanchos.Quiz.Dto.Abstractions;

namespace Quizanchos.Quiz.Dto;

public record QuizCardFloatDto(
    Guid Id,
    int CardIndex,
    int? OptionPicked,
    DateTime CreationTime,
    Guid[] EntitiesId
) : QuizCardDtoAbstract(Id, CardIndex, OptionPicked, CreationTime, EntitiesId);
