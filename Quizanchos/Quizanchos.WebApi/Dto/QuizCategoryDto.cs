using Quizanchos.Common.Enums;

namespace Quizanchos.WebApi.Dto;

public record BaseQuizCategoryDto(string Name, FeatureType FeatureType, string ImageUrl);

public record QuizCategoryDto(Guid Id, string Name, FeatureType FeatureType, string ImageUrl) : BaseQuizCategoryDto(Name, FeatureType, ImageUrl);
