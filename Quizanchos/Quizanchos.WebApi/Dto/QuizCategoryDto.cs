namespace Quizanchos.WebApi.Dto;

public class BaseQuizCategoryDto
{
    public string Name { get; set; } = string.Empty;
}

public class QuizCategoryDto : BaseQuizCategoryDto
{
    public Guid Id { get; set; }
}
