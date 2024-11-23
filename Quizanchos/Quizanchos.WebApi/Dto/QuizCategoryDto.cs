using Quizanchos.Common.Enums;

namespace Quizanchos.WebApi.Dto;

public record BaseQuizCategoryDto(string Name, FeatureType FeatureType);

public record QuizCategoryDto(Guid Id, string Name, FeatureType FeatureType) : BaseQuizCategoryDto(Name, FeatureType);
