namespace Quizanchos.WebApi.Dto;

public record ApplicationUserDto(string UserName, string AvatarUrl, int Score);

public record ApplicationUserListDto(List<ApplicationUserDto> Users);