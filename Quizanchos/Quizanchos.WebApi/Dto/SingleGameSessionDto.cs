namespace Quizanchos.WebApi.Dto;

public record BaseSingleGameSessionDto(Guid QuizCategoryId);

public record SingleGameSessionDto(
    Guid Id, 
    Guid QuizCategoryId, 
    string UserId,
    DateTime CreationTime,
    int CurrentQuestionIndex,
    int Score,
    bool IsFinished,
    int QuestionsCount
) : BaseSingleGameSessionDto(QuizCategoryId);
