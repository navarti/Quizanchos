using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Quizanchos.Common.Util;
using Quizanchos.Domain;
using Quizanchos.Domain.Entities;
using Quizanchos.WebApi.Dto;
using System.Security.Claims;

namespace Quizanchos.WebApi.Services.Users;

public class UserProfileService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserRetrieverService _userRetrieverService;
    private readonly ICloudinary _cloudinary;
    private readonly UserScoreService _userScoreService;
    private readonly QuizanchosDbContext _dbContext;

    public UserProfileService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        UserRetrieverService userRetrieverService,
        ICloudinary cloudinary,
        UserScoreService userScoreService,
        QuizanchosDbContext dbContext)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _userRetrieverService = userRetrieverService;
        _cloudinary = cloudinary;
        _userScoreService = userScoreService;
        _dbContext = dbContext;
    }

    public async Task<FullApplicationUserDto> GetUserInfo(ClaimsPrincipal claimsPrincipal)
    {
        ApplicationUser user = await _userRetrieverService.GetUserByClaims(claimsPrincipal).ConfigureAwait(false);
        int totalScore = await _userScoreService.GetTotalScoreAsync(user.Id).ConfigureAwait(false);
        return new FullApplicationUserDto(user.Email!, user.UserName!, user.AvatarUrl, totalScore, user.Coins, user.Status, user.PremiumUntilUtc);
    }

    public async Task UpdateNickname(ClaimsPrincipal claimsPrincipal, string nickNameToUpdate)
    {
        ApplicationUser user = await _userRetrieverService.GetUserByClaims(claimsPrincipal).ConfigureAwait(false);
        if(user.UserName == nickNameToUpdate)
        {
            return;
        }

        ApplicationUser? userWithNickname = await _userManager.FindByNameAsync(nickNameToUpdate);
        if(userWithNickname is not null)
        {
            throw HandledExceptionFactory.Create("The user with this nickname exists");
        }

        user.UserName = nickNameToUpdate;
        await _userManager.UpdateAsync(user).ConfigureAwait(false);
    }

    public async Task UpdateAvatarUrl(ClaimsPrincipal claimsPrincipal, string avatarUrl)
    {
        ApplicationUser user = await _userRetrieverService.GetUserByClaims(claimsPrincipal).ConfigureAwait(false);
        user.AvatarUrl = avatarUrl;
        await _userManager.UpdateAsync(user).ConfigureAwait(false);
    }

    private const long MaxAvatarSizeBytes = 5L * 1024 * 1024;

    private static readonly HashSet<string> AllowedAvatarContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/png", "image/jpeg", "image/webp", "image/gif"
    };

    private static readonly HashSet<string> AllowedAvatarExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".webp", ".gif"
    };

    public async Task UpdateAvatar(ClaimsPrincipal claimsPrincipal, IFormFile formFile)
    {
        ApplicationUser user = await _userRetrieverService.GetUserByClaims(claimsPrincipal).ConfigureAwait(false);

        if (formFile is null || formFile.Length == 0)
            throw HandledExceptionFactory.Create("No file uploaded.");

        if (formFile.Length > MaxAvatarSizeBytes)
            throw HandledExceptionFactory.Create("Avatar must be 5 MB or smaller.");

        if (!AllowedAvatarContentTypes.Contains(formFile.ContentType ?? string.Empty))
            throw HandledExceptionFactory.Create("Unsupported image format. Use PNG, JPEG, WebP, or GIF.");

        string extension = Path.GetExtension(formFile.FileName ?? string.Empty);
        if (!AllowedAvatarExtensions.Contains(extension))
            throw HandledExceptionFactory.Create("Unsupported image format. Use PNG, JPEG, WebP, or GIF.");

        using Stream stream = formFile.OpenReadStream();
        ImageUploadParams uploadParams = new ImageUploadParams
        {
            File = new FileDescription(formFile.FileName, stream),
            Transformation = new Transformation().Width(512).Height(512).Crop("limit")
        };

        ImageUploadResult uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult?.StatusCode != System.Net.HttpStatusCode.OK)
            throw HandledExceptionFactory.Create("Error uploading image.");

        user.AvatarUrl = uploadResult.SecureUrl.AbsoluteUri;
        await _userManager.UpdateAsync(user).ConfigureAwait(false);
    }

    public async Task AddCoins(ClaimsPrincipal claimsPrincipal, int coinsToAdd)
    {
        if (coinsToAdd <= 0)
        {
            throw HandledExceptionFactory.Create("Coins to add must be greater than zero");
        }

        ApplicationUser user = await _userRetrieverService.GetUserByClaims(claimsPrincipal).ConfigureAwait(false);
        user.Coins += coinsToAdd;
        await _userManager.UpdateAsync(user).ConfigureAwait(false);
    }

    public async Task<UserDataExportDto> ExportDataAsync(ClaimsPrincipal claimsPrincipal)
    {
        ApplicationUser user = await _userRetrieverService.GetUserByClaims(claimsPrincipal).ConfigureAwait(false);

        var scores = await _dbContext.UserMinigameScores
            .Where(s => s.ApplicationUserId == user.Id)
            .Select(s => new MinigameScoreExportDto(s.MinigameType, s.Score))
            .ToListAsync()
            .ConfigureAwait(false);

        var ownedItems = await _dbContext.UserOwnedItems
            .Where(o => o.ApplicationUserId == user.Id)
            .Select(o => new OwnedItemExportDto(
                o.Id,
                o.MarketItemId,
                o.MarketItem.Name,
                o.MarketItem.Type,
                o.PurchasedAtUtc))
            .ToListAsync()
            .ConfigureAwait(false);

        var gameSessions = await _dbContext.GameSessionPlayers
            .Where(p => p.ApplicationUserId == user.Id)
            .Select(p => new GameSessionExportDto(
                p.GameSession.Id,
                p.GameSession.MinigameType,
                p.GameSession.CreatedAt,
                p.GameSession.FinishedAt,
                p.GameSession.IsFinished,
                p.GameSession.WinnerId == user.Id,
                p.JoinedAt))
            .ToListAsync()
            .ConfigureAwait(false);

        var topUpOrders = await _dbContext.TopUpOrders
            .Where(o => o.ApplicationUserId == user.Id)
            .Select(o => new TopUpOrderExportDto(
                o.Id,
                o.Status,
                o.CoinsToCredit,
                o.AmountUSDT,
                o.Network,
                o.CreatedAtUtc,
                o.CompletedAtUtc,
                o.BinanceTxId))
            .ToListAsync()
            .ConfigureAwait(false);

        var profile = new UserProfileExportDto(
            user.Id,
            user.Email ?? string.Empty,
            user.UserName ?? string.Empty,
            user.AvatarUrl,
            user.Coins,
            user.Status,
            user.PremiumUntilUtc,
            user.EmailConfirmed);

        return new UserDataExportDto(profile, scores, ownedItems, gameSessions, topUpOrders);
    }

    public async Task DeleteAccountAsync(ClaimsPrincipal claimsPrincipal, string currentPassword)
    {
        ApplicationUser user = await _userRetrieverService.GetUserByClaims(claimsPrincipal).ConfigureAwait(false);

        if (!await _userManager.CheckPasswordAsync(user, currentPassword).ConfigureAwait(false))
        {
            throw HandledExceptionFactory.Create("Current password is incorrect.");
        }

        // TopUpOrder rows are configured with OnDelete(Restrict) to preserve financial
        // history. Surface a clear message rather than letting EF throw a foreign-key
        // exception. Users with order history must contact support for deletion.
        bool hasOrders = await _dbContext.TopUpOrders
            .AnyAsync(o => o.ApplicationUserId == user.Id)
            .ConfigureAwait(false);
        if (hasOrders)
        {
            throw HandledExceptionFactory.Create(
                "Cannot delete account with order history. Please contact support.");
        }

        IdentityResult result = await _userManager.DeleteAsync(user).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            throw HandledExceptionFactory.Create(string.Concat(result.Errors.Select(e => e.Description)));
        }

        await _signInManager.SignOutAsync().ConfigureAwait(false);
    }
}
