using Quizanchos.Common.Enums;

namespace Quizanchos.WebApi.Dto;

public record BaseSingleGameSessionDto(Guid QuizCategoryId, GameLevel GameLevel);

public record SingleGameSessionDto(
    Guid Id, 
    Guid QuizCategoryId,
    GameLevel GameLevel,
    string UserId,
    DateTime CreationTime,
    int CurrentCardIndex,
    int Score,
    bool IsFinished,
    int CardsCount
) : BaseSingleGameSessionDto(QuizCategoryId, GameLevel);
