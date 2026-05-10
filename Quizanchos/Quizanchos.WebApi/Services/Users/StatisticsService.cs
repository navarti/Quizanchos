using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Quizanchos.Common.Util;
using Quizanchos.Core;
using Quizanchos.Domain;
using Quizanchos.Domain.Entities;
using Quizanchos.WebApi.Dto.Statistics;
using Quizanchos.WebApi.Services.Rooms;

namespace Quizanchos.WebApi.Services.Users;

/// <summary>
/// Aggregates platform usage data for the admin statistics dashboard.
/// All time arguments are interpreted as UTC.
/// </summary>
public class StatisticsService
{
    public enum BucketKind { Hour, Day, Week, Month }

    private const int MaxBuckets = 366;

    private readonly QuizanchosDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMinigameRegistry _registry;
    private readonly IGameRoomManager _roomManager;

    public StatisticsService(
        QuizanchosDbContext db,
        UserManager<ApplicationUser> userManager,
        IMinigameRegistry registry,
        IGameRoomManager roomManager)
    {
        _db = db;
        _userManager = userManager;
        _registry = registry;
        _roomManager = roomManager;
    }

    public IReadOnlyList<StatisticsGameInfoDto> GetRegisteredGames()
    {
        return _registry.GetAllDescriptors().Values
            .OrderBy(d => d.MinigameTypeId)
            .Select(d => new StatisticsGameInfoDto(d.MinigameTypeId, d.GameKey, d.DisplayName))
            .ToList();
    }

    public async Task<StatisticsOverviewDto> GetOverviewAsync(DateTime fromUtc, DateTime toUtc)
    {
        ValidateRange(fromUtc, toUtc);

        var inRange = _db.GameSessions.Where(s => s.CreatedAt >= fromUtc && s.CreatedAt < toUtc);

        int totalSessions = await inRange.CountAsync().ConfigureAwait(false);
        int finishedSessions = await inRange.CountAsync(s => s.IsFinished).ConfigureAwait(false);

        int distinctPlayers = await _db.GameSessionPlayers
            .Where(p => p.GameSession.CreatedAt >= fromUtc && p.GameSession.CreatedAt < toUtc)
            .Select(p => p.ApplicationUserId)
            .Distinct()
            .CountAsync()
            .ConfigureAwait(false);

        // Identity users don't carry a created-at column, so approximate "new users in range"
        // as users whose first-ever session join falls inside the range. Off by people who
        // registered but never played, but a reasonable proxy for engagement.
        int newUsers = await _db.GameSessionPlayers
            .GroupBy(p => p.ApplicationUserId)
            .Select(g => g.Min(p => p.JoinedAt))
            .CountAsync(firstJoin => firstJoin >= fromUtc && firstJoin < toUtc)
            .ConfigureAwait(false);

        // Compute average session duration client-side because Npgsql doesn't expose
        // EF.Functions.DateDiffSecond. The set of finished sessions in any reasonable
        // admin range is small enough to materialize.
        var finishedTimes = await inRange
            .Where(s => s.IsFinished && s.FinishedAt != null)
            .Select(s => new { s.CreatedAt, s.FinishedAt })
            .ToListAsync()
            .ConfigureAwait(false);

        double avgDuration = 0;
        if (finishedTimes.Count > 0)
        {
            var positive = finishedTimes
                .Select(x => (x.FinishedAt!.Value - x.CreatedAt).TotalSeconds)
                .Where(s => s > 0)
                .ToList();
            avgDuration = positive.Count > 0 ? positive.Average() : 0;
        }

        double avgConcurrent = await ComputeAverageConcurrentSessionsAsync(fromUtc, toUtc).ConfigureAwait(false);

        double completionRate = totalSessions > 0
            ? Math.Round(100.0 * finishedSessions / totalSessions, 1)
            : 0;

        int liveActiveSessions = await _db.GameSessions
            .CountAsync(s => s.IsActive && !s.IsFinished)
            .ConfigureAwait(false);

        var lobbyRooms = _roomManager.GetAvailableRooms();
        int liveLobbyRooms = lobbyRooms.Count;
        int liveLobbyPlayers = lobbyRooms.Sum(r => r.AllPlayerIds.Count);

        int totalUsers = await _userManager.Users.CountAsync().ConfigureAwait(false);
        int totalSessionsAllTime = await _db.GameSessions.CountAsync().ConfigureAwait(false);

        return new StatisticsOverviewDto(
            fromUtc,
            toUtc,
            totalSessions,
            finishedSessions,
            distinctPlayers,
            newUsers,
            Math.Round(avgConcurrent, 2),
            Math.Round(avgDuration, 1),
            completionRate,
            liveActiveSessions,
            liveLobbyRooms,
            liveLobbyPlayers,
            totalUsers,
            totalSessionsAllTime);
    }

    public async Task<List<SessionsByGameDto>> GetSessionsByGameAsync(DateTime fromUtc, DateTime toUtc)
    {
        ValidateRange(fromUtc, toUtc);

        var inRange = _db.GameSessions.Where(s => s.CreatedAt >= fromUtc && s.CreatedAt < toUtc);

        var perGameRaw = await inRange
            .GroupBy(s => s.MinigameType)
            .Select(g => new
            {
                MinigameType = g.Key,
                Total = g.Count(),
                Finished = g.Count(s => s.IsFinished),
            })
            .ToListAsync()
            .ConfigureAwait(false);

        var finishedTimesByGame = await inRange
            .Where(s => s.IsFinished && s.FinishedAt != null)
            .Select(s => new { s.MinigameType, s.CreatedAt, s.FinishedAt })
            .ToListAsync()
            .ConfigureAwait(false);

        var avgDurationByGame = finishedTimesByGame
            .GroupBy(x => x.MinigameType)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var positive = g.Select(x => (x.FinishedAt!.Value - x.CreatedAt).TotalSeconds)
                        .Where(s => s > 0)
                        .ToList();
                    return positive.Count > 0 ? positive.Average() : 0;
                });

        var distinctPlayersByGame = await _db.GameSessionPlayers
            .Where(p => p.GameSession.CreatedAt >= fromUtc && p.GameSession.CreatedAt < toUtc)
            .GroupBy(p => p.GameSession.MinigameType)
            .Select(g => new { MinigameType = g.Key, Players = g.Select(x => x.ApplicationUserId).Distinct().Count() })
            .ToListAsync()
            .ConfigureAwait(false);

        var liveByGame = await _db.GameSessions
            .Where(s => s.IsActive && !s.IsFinished)
            .GroupBy(s => s.MinigameType)
            .Select(g => new { MinigameType = g.Key, Live = g.Count() })
            .ToListAsync()
            .ConfigureAwait(false);

        int total = perGameRaw.Sum(x => x.Total);
        var descriptors = _registry.GetAllDescriptors().Values.ToDictionary(d => d.MinigameTypeId);

        return perGameRaw
            .Select(row =>
            {
                descriptors.TryGetValue(row.MinigameType, out var descriptor);
                int distinct = distinctPlayersByGame.FirstOrDefault(x => x.MinigameType == row.MinigameType)?.Players ?? 0;
                int live = liveByGame.FirstOrDefault(x => x.MinigameType == row.MinigameType)?.Live ?? 0;
                double share = total > 0 ? Math.Round(100.0 * row.Total / total, 1) : 0;
                avgDurationByGame.TryGetValue(row.MinigameType, out double avg);
                return new SessionsByGameDto(
                    row.MinigameType,
                    descriptor?.GameKey ?? $"unknown-{row.MinigameType}",
                    descriptor?.DisplayName ?? $"Unknown ({row.MinigameType})",
                    row.Total,
                    row.Finished,
                    distinct,
                    live,
                    Math.Round(avg, 1),
                    share);
            })
            .OrderByDescending(x => x.TotalSessions)
            .ToList();
    }

    public async Task<SessionsTimeSeriesDto> GetSessionsTimeSeriesAsync(
        DateTime fromUtc,
        DateTime toUtc,
        BucketKind bucket,
        int? minigameType)
    {
        ValidateRange(fromUtc, toUtc);

        var buckets = BuildBuckets(fromUtc, toUtc, bucket);
        if (buckets.Count == 0)
        {
            return new SessionsTimeSeriesDto(fromUtc, toUtc, bucket.ToString(), buckets, new());
        }

        var query = _db.GameSessions.Where(s => s.CreatedAt >= fromUtc && s.CreatedAt < toUtc);
        if (minigameType.HasValue)
        {
            query = query.Where(s => s.MinigameType == minigameType.Value);
        }

        var rows = await query
            .Select(s => new { s.MinigameType, s.CreatedAt })
            .ToListAsync()
            .ConfigureAwait(false);

        var descriptors = _registry.GetAllDescriptors().Values.ToDictionary(d => d.MinigameTypeId);

        var grouped = rows
            .GroupBy(r => r.MinigameType)
            .Select(group =>
            {
                descriptors.TryGetValue(group.Key, out var descriptor);
                var counts = buckets.ToDictionary(b => b, _ => 0);
                foreach (var row in group)
                {
                    var key = AlignToBucket(row.CreatedAt, bucket);
                    if (counts.ContainsKey(key))
                    {
                        counts[key]++;
                    }
                }
                return new GameTimeSeriesDto(
                    group.Key,
                    descriptor?.GameKey ?? $"unknown-{group.Key}",
                    descriptor?.DisplayName ?? $"Unknown ({group.Key})",
                    buckets.Select(b => new TimeSeriesPointDto(b, counts[b])).ToList());
            })
            .OrderByDescending(s => s.Points.Sum(p => p.Count))
            .ToList();

        return new SessionsTimeSeriesDto(fromUtc, toUtc, bucket.ToString(), buckets, grouped);
    }

    public async Task<List<TopPlayerDto>> GetTopPlayersAsync(DateTime fromUtc, DateTime toUtc, int limit)
    {
        ValidateRange(fromUtc, toUtc);
        if (limit <= 0 || limit > 100) limit = 10;

        var topUserIds = await _db.GameSessionPlayers
            .Where(p => p.GameSession.CreatedAt >= fromUtc && p.GameSession.CreatedAt < toUtc)
            .GroupBy(p => p.ApplicationUserId)
            .Select(g => new
            {
                UserId = g.Key,
                Sessions = g.Count(),
                Wins = g.Count(p => p.GameSession.WinnerId == p.ApplicationUserId),
            })
            .OrderByDescending(x => x.Sessions)
            .Take(limit)
            .ToListAsync()
            .ConfigureAwait(false);

        if (topUserIds.Count == 0) return new();

        var userIds = topUserIds.Select(x => x.UserId).ToList();

        var users = await _userManager.Users
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.UserName, u.AvatarUrl })
            .ToListAsync()
            .ConfigureAwait(false);

        var totalScores = await _db.UserMinigameScores
            .Where(s => userIds.Contains(s.ApplicationUserId))
            .GroupBy(s => s.ApplicationUserId)
            .Select(g => new { UserId = g.Key, Total = g.Sum(s => s.Score) })
            .ToListAsync()
            .ConfigureAwait(false);

        return topUserIds
            .Select(row =>
            {
                var user = users.FirstOrDefault(u => u.Id == row.UserId);
                int score = totalScores.FirstOrDefault(s => s.UserId == row.UserId)?.Total ?? 0;
                return new TopPlayerDto(
                    user?.UserName ?? "(deleted)",
                    user?.AvatarUrl ?? "",
                    row.Sessions,
                    row.Wins,
                    score);
            })
            .ToList();
    }

    /// <summary>
    /// Average concurrent active sessions over the range = sum of seconds each session
    /// was alive within [fromUtc, toUtc] divided by the range duration.
    /// </summary>
    private async Task<double> ComputeAverageConcurrentSessionsAsync(DateTime fromUtc, DateTime toUtc)
    {
        var rangeSeconds = (toUtc - fromUtc).TotalSeconds;
        if (rangeSeconds <= 0) return 0;

        DateTime nowUtc = DateTime.UtcNow;
        DateTime cap = toUtc < nowUtc ? toUtc : nowUtc;

        var overlaps = await _db.GameSessions
            .Where(s => s.CreatedAt < toUtc
                && (s.FinishedAt == null || s.FinishedAt > fromUtc))
            .Select(s => new
            {
                s.CreatedAt,
                s.FinishedAt,
            })
            .ToListAsync()
            .ConfigureAwait(false);

        double totalCoverageSeconds = 0;
        foreach (var s in overlaps)
        {
            DateTime start = s.CreatedAt < fromUtc ? fromUtc : s.CreatedAt;
            DateTime end = s.FinishedAt ?? cap;
            if (end > toUtc) end = toUtc;
            if (end > start)
            {
                totalCoverageSeconds += (end - start).TotalSeconds;
            }
        }

        return totalCoverageSeconds / rangeSeconds;
    }

    private static void ValidateRange(DateTime fromUtc, DateTime toUtc)
    {
        if (toUtc <= fromUtc)
        {
            throw HandledExceptionFactory.Create("End date must be after start date.");
        }
        if ((toUtc - fromUtc).TotalDays > 400)
        {
            throw HandledExceptionFactory.Create("Range must be at most 400 days.");
        }
    }

    private static List<DateTime> BuildBuckets(DateTime fromUtc, DateTime toUtc, BucketKind bucket)
    {
        var start = AlignToBucket(fromUtc, bucket);
        var result = new List<DateTime>();
        var cursor = start;
        while (cursor < toUtc && result.Count < MaxBuckets)
        {
            result.Add(cursor);
            cursor = bucket switch
            {
                BucketKind.Hour => cursor.AddHours(1),
                BucketKind.Day => cursor.AddDays(1),
                BucketKind.Week => cursor.AddDays(7),
                BucketKind.Month => cursor.AddMonths(1),
                _ => cursor.AddDays(1),
            };
        }
        return result;
    }

    private static DateTime AlignToBucket(DateTime value, BucketKind bucket) => bucket switch
    {
        BucketKind.Hour => new DateTime(value.Year, value.Month, value.Day, value.Hour, 0, 0, value.Kind),
        BucketKind.Day => new DateTime(value.Year, value.Month, value.Day, 0, 0, 0, value.Kind),
        BucketKind.Week => StartOfWeek(value),
        BucketKind.Month => new DateTime(value.Year, value.Month, 1, 0, 0, 0, value.Kind),
        _ => value.Date,
    };

    private static DateTime StartOfWeek(DateTime value)
    {
        int diff = (7 + (value.DayOfWeek - DayOfWeek.Monday)) % 7;
        return new DateTime(value.Year, value.Month, value.Day, 0, 0, 0, value.Kind).AddDays(-diff);
    }
}
