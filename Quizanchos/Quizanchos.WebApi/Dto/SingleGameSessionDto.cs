using Quizanchos.Common.Enums;

namespace Quizanchos.WebApi.Dto;

public record BaseSingleGameSessionDto(Guid QuizCategoryId, GameLevel GameLevel, CardCountEnum CardsCount, SecondsPerCardEnum SecondPerCard, OptionCountEnum OptionCount);

public record SingleGameSessionDto(
    Guid Id, 
    Guid QuizCategoryId,
    GameLevel GameLevel,
    string UserId,
    DateTime CreationTime,
    int CurrentCardIndex,
    int Score,
    bool IsFinished,
    bool IsTerminatedByTime,
    CardCountEnum CardsCount,
    SecondsPerCardEnum SecondPerCard,
    OptionCountEnum OptionCount
) : BaseSingleGameSessionDto(QuizCategoryId, GameLevel, CardsCount, SecondPerCard, OptionCount);
