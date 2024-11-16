namespace Quizanchos.WebApi.Dto;

public record BaseSingleGameSessionDto(Guid QuizCategoryId);

public record SingleGameSessionDto(
    Guid Id, 
    Guid QuizCategoryId, 
    string UserId
) : BaseSingleGameSessionDto(QuizCategoryId);
