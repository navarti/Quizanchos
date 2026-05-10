using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Quizanchos.Common.Util;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain;
using Quizanchos.Quiz.Util;
using Quizanchos.WebApi.Dto;

namespace Quizanchos.WebApi.Services.Users;

public class AdminService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly QuizanchosDbContext _dbContext;

    public AdminService(UserManager<ApplicationUser> userManager, QuizanchosDbContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<AdminUserDto>> GetUsersAsync(string name, int take, int skip)
    {
        SkipTakeValidator.Validate(skip, take);

        IQueryable<ApplicationUser> users = _userManager.Users;

        if (!string.IsNullOrEmpty(name))
            users = users.Where(u => u.UserName!.StartsWith(name));

        var usersList = await users.Skip(skip).Take(take).ToListAsync().ConfigureAwait(false);

        // Get all scores for these users
        var userIds = usersList.Select(u => u.Id).ToList();
        var scores = await _dbContext.UserMinigameScores
            .Where(s => userIds.Contains(s.ApplicationUserId))
            .ToListAsync()
            .ConfigureAwait(false);

        return usersList.Select(u => new AdminUserDto(
            u.Email!,
            u.UserName!,
            u.AvatarUrl,
            scores.Where(s => s.ApplicationUserId == u.Id).Sum(s => s.Score),
            u.Coins,
            u.Status));
    }

    public async Task DeleteUser(string email)
    {
        ApplicationUser? user = await _userManager.FindByEmailAsync(email);
        _ = user ?? throw HandledExceptionFactory.Create("The user with this id does not exist");
        await _userManager.DeleteAsync(user);
    }

    public async Task UpdateUserNickname(string email, string newNickname)
    {
        if (string.IsNullOrWhiteSpace(newNickname))
        {
            throw HandledExceptionFactory.Create("Nickname cannot be empty");
        }

        ApplicationUser user = await _userManager.FindByEmailAsync(email)
            ?? throw HandledExceptionFactory.Create("The user with this email does not exist");

        if (user.UserName == newNickname)
        {
            return;
        }

        ApplicationUser? userWithNickname = await _userManager.FindByNameAsync(newNickname);
        if (userWithNickname is not null)
        {
            throw HandledExceptionFactory.Create("The user with this nickname exists");
        }

        user.UserName = newNickname;
        IdentityResult result = await _userManager.UpdateAsync(user).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            throw HandledExceptionFactory.Create(string.Concat(result.Errors.Select(e => e.Description)));
        }
    }
}
