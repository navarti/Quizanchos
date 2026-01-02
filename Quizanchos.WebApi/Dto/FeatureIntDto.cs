namespace Quizanchos.WebApi.Dto;

public record BaseFeatureIntDto(int Value, Guid QuizCategoryId, Guid QuizEntityId);

public record FeatureIntDto(Guid Id, int Value, Guid QuizCategoryId, Guid QuizEntityId);
