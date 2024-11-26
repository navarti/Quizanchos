using Microsoft.AspNetCore.Identity;

namespace Quizanchos.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string AvatarUrl { get; set; } = "";
}
