using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Quizanchos.Domain.Entities;
using Quizanchos.WebApi.Dto;
using Quizanchos.Quiz.Util;
using System.Security.Claims;
using System.Linq;
using Quizanchos.Domain;


namespace Quizanchos.WebApi.Services.Users;

public class LeaderBoardService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly UserRetrieverService _userRetrieverService;
    private readonly QuizanchosDbContext _dbContext;
    private readonly Quizanchos.Domain.Repositories.Interfaces.IUserMinigameScoreRepository _scoreRepository;

    public LeaderBoardService(UserManager<ApplicationUser> userManager, UserRetrieverService userRetrieverService, QuizanchosDbContext dbContext, Quizanchos.Domain.Repositories.Interfaces.IUserMinigameScoreRepository scoreRepository)
    {
        _userManager = userManager;
        _userRetrieverService = userRetrieverService;
        _dbContext = dbContext;
        _scoreRepository = scoreRepository;
    }
    public async Task<List<ApplicationUserInLeaderBoardDto>> GetLeaderBoardAsync(int take, int skip)
    {
        return await GetLeaderBoardAsync(take, skip, null).ConfigureAwait(false);
    }

    public async Task<List<ApplicationUserInLeaderBoardDto>> GetLeaderBoardAsync(int take, int skip, int? minigameType)
    {
        var users = await GetAppUsesLeaderBoardAsync(take, skip, minigameType).ConfigureAwait(false);

        var userIds = users.Select(u => u.Id).ToList();
        var scores = (await _scoreRepository.Get(
            whereExpression: s => userIds.Contains(s.ApplicationUserId) && (!minigameType.HasValue || s.MinigameType == minigameType.Value),
            asNoTracking: true).ToListAsync().ConfigureAwait(false));

        return users.Select((user, position) =>
        {
            int score;
            if (minigameType.HasValue)
            {
                score = scores.FirstOrDefault(s => s.ApplicationUserId == user.Id)?.Score ?? 0;
            }
            else
            {
                score = scores.Where(s => s.ApplicationUserId == user.Id).Sum(s => s.Score);
            }

            return new ApplicationUserInLeaderBoardDto(user.UserName!, user.AvatarUrl, score, user.Coins, position + 1, user.Status);
        }).ToList();
    }

    public async Task<List<ApplicationUser>> GetAppUsesLeaderBoardAsync(int take, int skip)
    {
        return await GetAppUsesLeaderBoardAsync(take, skip, null).ConfigureAwait(false);
    }

    public async Task<List<ApplicationUser>> GetAppUsesLeaderBoardAsync(int take, int skip, int? minigameType)
    {
        SkipTakeValidator.Validate(skip, take);

        // Load users and scores into memory, then order in-memory to support per-minigame ranking
        var users = await _dbContext.Users.ToListAsync().ConfigureAwait(false);
        var scores = await _dbContext.UserMinigameScores
            .Where(s => !minigameType.HasValue || s.MinigameType == minigameType.Value)
            .ToListAsync()
            .ConfigureAwait(false);

        IEnumerable<ApplicationUser> ordered = minigameType.HasValue
            ? users.OrderByDescending(u => scores.FirstOrDefault(s => s.ApplicationUserId == u.Id)?.Score ?? 0)
            : users.OrderByDescending(u => scores.Where(s => s.ApplicationUserId == u.Id).Sum(s => s.Score));

        return ordered.Skip(skip).Take(take).ToList();
    }

    public async Task<ApplicationUserInLeaderBoardDto> GetUserPositionAsync(ClaimsPrincipal claimsPrincipal)
    {
        return await GetUserPositionAsync(claimsPrincipal, null).ConfigureAwait(false);
    }

    public async Task<ApplicationUserInLeaderBoardDto> GetUserPositionAsync(ClaimsPrincipal claimsPrincipal, int? minigameType)
    {
        ApplicationUser user = await _userRetrieverService.GetUserByClaims(claimsPrincipal).ConfigureAwait(false);
        // Use DB scores to order
        var usersWithScores = await _dbContext.Users
            .Select(u => new
            {
                User = u,
                Scores = _dbContext.UserMinigameScores.Where(s => s.ApplicationUserId == u.Id)
            })
            .ToListAsync();

        var ordered = minigameType.HasValue
            ? usersWithScores.OrderByDescending(x => x.Scores.FirstOrDefault(s => s.MinigameType == minigameType.Value)?.Score ?? 0).Select(x => x.User).ToList()
            : usersWithScores.OrderByDescending(x => x.Scores.Sum(s => s.Score)).Select(x => x.User).ToList();

        int position = ordered.FindIndex(u => u.Id == user.Id);

        // compute user's score from DB
        var userScores = await _dbContext.UserMinigameScores
            .Where(s => s.ApplicationUserId == user.Id && (!minigameType.HasValue || s.MinigameType == minigameType.Value))
            .ToListAsync()
            .ConfigureAwait(false);

        var score = minigameType.HasValue ? userScores.FirstOrDefault()?.Score ?? 0 : userScores.Sum(s => s.Score);

        return new ApplicationUserInLeaderBoardDto(user.UserName!, user.AvatarUrl, score, user.Coins, position + 1, user.Status);
    }

}
