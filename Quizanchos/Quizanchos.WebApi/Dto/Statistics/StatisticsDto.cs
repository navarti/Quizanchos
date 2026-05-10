namespace Quizanchos.WebApi.Dto.Statistics;

/// <summary>
/// Headline KPI numbers for the admin statistics dashboard.
/// Time-bound metrics ("InRange") respect the from/to filter; "Live" metrics are
/// snapshots of the current moment regardless of the chosen period.
/// </summary>
public record StatisticsOverviewDto(
    DateTime FromUtc,
    DateTime ToUtc,
    int TotalSessionsInRange,
    int FinishedSessionsInRange,
    int DistinctPlayersInRange,
    int NewUsersInRange,
    double AverageConcurrentSessionsInRange,
    double AverageSessionDurationSeconds,
    double CompletionRatePercent,
    int LiveActiveSessions,
    int LiveLobbyRooms,
    int LiveLobbyPlayers,
    int TotalUsers,
    int TotalSessionsAllTime);

public record SessionsByGameDto(
    int MinigameTypeId,
    string GameKey,
    string DisplayName,
    int TotalSessions,
    int FinishedSessions,
    int DistinctPlayers,
    int LiveActiveSessions,
    double AverageDurationSeconds,
    double SharePercent);

public record TimeSeriesPointDto(
    DateTime BucketStartUtc,
    int Count);

public record GameTimeSeriesDto(
    int MinigameTypeId,
    string GameKey,
    string DisplayName,
    List<TimeSeriesPointDto> Points);

public record SessionsTimeSeriesDto(
    DateTime FromUtc,
    DateTime ToUtc,
    string Bucket,
    List<DateTime> Buckets,
    List<GameTimeSeriesDto> Series);

public record TopPlayerDto(
    string UserName,
    string AvatarUrl,
    int SessionsPlayed,
    int SessionsWon,
    int TotalScore);

public record StatisticsGameInfoDto(
    int MinigameTypeId,
    string GameKey,
    string DisplayName);
