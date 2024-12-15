using Quizanchos.Common.Enums;

namespace Quizanchos.WebApi.Dto;

public record ApplicationUserDto(string UserName, string AvatarUrl, int Score, UserStatusEnum UserStatus);

public record FullApplicationUserDto(string Email, string UserName, string AvatarUrl, int Score, UserStatusEnum UserStatus)
    : ApplicationUserDto(UserName, AvatarUrl, Score, UserStatus);

public record ApplicationUserInLeaderBoardDto(string UserName, string AvatarUrl, int Score, int Position, UserStatusEnum UserStatus)
    : ApplicationUserDto(UserName, AvatarUrl, Score, UserStatus);