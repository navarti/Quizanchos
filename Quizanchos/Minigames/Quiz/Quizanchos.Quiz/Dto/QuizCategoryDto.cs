using Quizanchos.Common.Quiz.Enums;

namespace Quizanchos.Quiz.Dto;

public record BaseQuizCategoryDto(string Name, FeatureType FeatureType, string ImageUrl, string AuthorName, 
    DateTime CreationDate, string QuestionToDisplay, bool IsPremium);

public record QuizCategoryDto(Guid Id, string Name, FeatureType FeatureType, string ImageUrl, string AuthorName, 
        DateTime CreationDate, string QuestionToDisplay, bool IsPremium) 
    : BaseQuizCategoryDto(Name, FeatureType, ImageUrl, AuthorName, CreationDate, QuestionToDisplay, IsPremium);
