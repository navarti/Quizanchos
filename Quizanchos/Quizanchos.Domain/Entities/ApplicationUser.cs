using Microsoft.AspNetCore.Identity;
using Quizanchos.Common.Enums;

namespace Quizanchos.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string AvatarUrl { get; set; } = "";

    public int Coins { get; set; } = 0;

    public DateTime? PremiumUntilUtc { get; set; }

    public UserStatusEnum Status { get; set; }

    // Per-minigame scores are stored as separate entity "UserMinigameScore"
    public List<UserMinigameScore> MinigameScores { get; set; } = new();

    public List<UserOwnedItem> OwnedItems { get; set; } = new();
}
