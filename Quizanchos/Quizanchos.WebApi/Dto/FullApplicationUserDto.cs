namespace Quizanchos.WebApi.Dto;

public record FullApplicationUserDto(string Email, string UserName, string AvatarUrl, int Score)
    : ApplicationUserDto(UserName, AvatarUrl, Score);
