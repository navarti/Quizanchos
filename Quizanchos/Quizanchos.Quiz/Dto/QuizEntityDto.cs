namespace Quizanchos.Quiz.Dto;

public record BaseQuizEntityDto(string Name);

public record QuizEntityDto(Guid Id, string Name) : BaseQuizEntityDto(Name);
