namespace Quizanchos.WebApi.Dto;

public class BaseQuizEntityDto
{
    public string Name { get; set; } = string.Empty;
}

public class QuizEntityDto : BaseQuizEntityDto
{
    public Guid Id { get; set; }
}
