namespace Quizanchos.WebApi.Dto;

public record BaseFeatureFloatDto(float Value, Guid QuizCategoryId, Guid QuizEntityId);

public record FeatureFloatDto(Guid Id, float Value, Guid QuizCategoryId, Guid QuizEntityId);
