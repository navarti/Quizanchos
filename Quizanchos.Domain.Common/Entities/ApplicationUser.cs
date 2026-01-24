using Microsoft.AspNetCore.Identity;
using Quizanchos.Common.Enums;

namespace Quizanchos.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string AvatarUrl { get; set; } = "";
    public int Score { get; set; }
    public UserStatusEnum Status { get; set; }
}
