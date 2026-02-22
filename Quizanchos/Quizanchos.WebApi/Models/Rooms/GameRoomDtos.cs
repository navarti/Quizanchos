using Quizanchos.Common.Enums;

namespace Quizanchos.WebApi.Models.Rooms;

public record CreateRoomRequest
{
    public MinigameType MinigameType { get; init; }
    public int MaxPlayers { get; init; }
    public int TeamCount { get; init; } = 1;
    public Dictionary<string, object>? GameParameters { get; init; }
}

public record JoinRoomRequest
{
    public int TeamIndex { get; init; }
}

public record SwitchTeamRequest
{
    public int NewTeamIndex { get; init; }
}

public record GameRoomTeamDto
{
    public int TeamIndex { get; init; }
    public string Name { get; init; } = string.Empty;
    public int MaxSize { get; init; }
    public IReadOnlyList<string> Players { get; init; } = Array.Empty<string>();
    public bool IsFull { get; init; }
}

public record GameRoomDto
{
    public Guid RoomId { get; init; }
    public MinigameType MinigameType { get; init; }
    public string HostPlayerId { get; init; } = string.Empty;
    public int MaxPlayers { get; init; }
    public int CurrentPlayerCount { get; init; }
    public GameRoomStatus Status { get; init; }
    public IReadOnlyList<GameRoomTeamDto> Teams { get; init; } = Array.Empty<GameRoomTeamDto>();
    public DateTime CreatedAt { get; init; }
    public Guid? LaunchedGameId { get; init; }
}

public record RoomActionResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public GameRoomDto? Room { get; init; }

    public static RoomActionResult Success(GameRoomDto room) =>
        new() { IsSuccess = true, Room = room };

    public static RoomActionResult Error(string message) =>
        new() { IsSuccess = false, ErrorMessage = message };
}

/// <summary>
/// Carries team composition information when a room launches a game.
/// </summary>
public record TeamInfo(int TeamIndex, string Name, IReadOnlyList<string> PlayerIds);
