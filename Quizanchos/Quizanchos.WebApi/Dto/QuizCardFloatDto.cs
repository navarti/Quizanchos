using Quizanchos.WebApi.Dto.Abstractions;

namespace Quizanchos.WebApi.Dto;

public record QuizCardFloatDto(
    Guid Id,
    int CardIndex,
    float Option1,
    float Option2,
    int OptionPicked
) : QuizCardDto<float>(Id, CardIndex, Option1, Option2, OptionPicked);
