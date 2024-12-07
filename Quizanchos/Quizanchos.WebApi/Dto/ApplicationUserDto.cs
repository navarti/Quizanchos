namespace Quizanchos.WebApi.Dto;

public record ApplicationUserDto(string UserName, string AvatarUrl, int Score);

public record FullApplicationUserDto(string Email, string UserName, string AvatarUrl, int Score)
    : ApplicationUserDto(UserName, AvatarUrl, Score);

public record ApplicationUserInLeaderBoardDto(string UserName, string AvatarUrl, int Score, int Position)
    : ApplicationUserDto(UserName, AvatarUrl, Score);