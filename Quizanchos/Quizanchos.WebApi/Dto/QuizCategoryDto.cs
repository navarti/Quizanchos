using Quizanchos.Common.Enums;

namespace Quizanchos.WebApi.Dto;

public record BaseQuizCategoryDto(string Name, FeatureType FeatureType, string ImageUrl, string AuthorName, DateTime CreationDate);

public record QuizCategoryDto(Guid Id, string Name, FeatureType FeatureType, string ImageUrl, string AuthorName, DateTime CreationDate) 
    : BaseQuizCategoryDto(Name, FeatureType, ImageUrl, AuthorName, CreationDate);
