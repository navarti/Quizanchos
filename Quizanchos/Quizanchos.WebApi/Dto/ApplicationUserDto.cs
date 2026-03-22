using Quizanchos.Common.Enums;

namespace Quizanchos.WebApi.Dto;

public record ApplicationUserDto(string UserName, string AvatarUrl, int Score, int Coins, UserStatusEnum UserStatus);

public record FullApplicationUserDto(string Email, string UserName, string AvatarUrl, int Score, int Coins, UserStatusEnum UserStatus)
    : ApplicationUserDto(UserName, AvatarUrl, Score, Coins, UserStatus);

public record ApplicationUserInLeaderBoardDto(string UserName, string AvatarUrl, int Score, int Coins, int Position, UserStatusEnum UserStatus)
    : ApplicationUserDto(UserName, AvatarUrl, Score, Coins, UserStatus);
