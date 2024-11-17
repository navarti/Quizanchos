namespace Quizanchos.WebApi.Dto;

public record BaseQuizCategoryDto(string Name);

public record QuizCategoryDto(Guid Id, string Name) : BaseQuizCategoryDto(Name);
