using Microsoft.AspNetCore.Identity;

namespace Quizanchos.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public ICollection<SingleGameSession> SingleGameSessions { get; }
}
