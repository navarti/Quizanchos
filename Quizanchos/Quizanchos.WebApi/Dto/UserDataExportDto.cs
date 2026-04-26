using Quizanchos.Common.Enums;
using Quizanchos.Domain.Entities;

namespace Quizanchos.WebApi.Dto;

public record UserDataExportDto(
    UserProfileExportDto Profile,
    IReadOnlyList<MinigameScoreExportDto> Scores,
    IReadOnlyList<OwnedItemExportDto> OwnedItems,
    IReadOnlyList<GameSessionExportDto> GameSessions,
    IReadOnlyList<TopUpOrderExportDto> TopUpOrders);

public record UserProfileExportDto(
    string Id,
    string Email,
    string UserName,
    string AvatarUrl,
    int Coins,
    UserStatusEnum Status,
    DateTime? PremiumUntilUtc,
    bool EmailConfirmed);

public record MinigameScoreExportDto(int MinigameType, int Score);

public record OwnedItemExportDto(
    Guid Id,
    Guid MarketItemId,
    string MarketItemName,
    MarketItemType MarketItemType,
    DateTime PurchasedAtUtc);

public record GameSessionExportDto(
    Guid Id,
    int MinigameType,
    DateTime CreatedAt,
    DateTime? FinishedAt,
    bool IsFinished,
    bool WonByThisUser,
    DateTime JoinedAt);

public record TopUpOrderExportDto(
    Guid Id,
    TopUpOrderStatus Status,
    int CoinsToCredit,
    decimal AmountUSDT,
    string Network,
    DateTime CreatedAtUtc,
    DateTime? CompletedAtUtc,
    string? BinanceTxId);

public record DeleteAccountDto(
    [System.ComponentModel.DataAnnotations.Required] string CurrentPassword);
