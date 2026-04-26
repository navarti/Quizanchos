namespace Quizanchos.WebApi.Models.Rooms;

public enum GameRoomStatus
{
    WaitingForPlayers,
    Launching,
    GameStarted,
    Closed
}

public class GameRoomTeam
{
    private readonly List<string> _players = new();

    public int TeamIndex { get; }
    public string Name { get; }
    public int MaxSize { get; }
    public IReadOnlyList<string> Players => _players.AsReadOnly();
    public bool IsFull => _players.Count >= MaxSize;

    public GameRoomTeam(int teamIndex, string name, int maxSize)
    {
        TeamIndex = teamIndex;
        Name = name;
        MaxSize = maxSize;
    }

    public bool AddPlayer(string playerId)
    {
        if (IsFull || _players.Contains(playerId))
            return false;

        _players.Add(playerId);
        return true;
    }

    public bool RemovePlayer(string playerId) => _players.Remove(playerId);

    public bool ContainsPlayer(string playerId) => _players.Contains(playerId);
}

public class GameRoom
{
    public static readonly TimeSpan JoinWindow = TimeSpan.FromMinutes(15);

    private readonly object _syncRoot = new();

    public Guid RoomId { get; }
    public int MinigameType { get; }
    public string HostPlayerId { get; }
    public int MaxPlayers { get; }
    public Dictionary<string, object>? GameParameters { get; }
    public List<GameRoomTeam> Teams { get; }
    public GameRoomStatus Status { get; set; } = GameRoomStatus.WaitingForPlayers;
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    public Guid? LaunchedGameId { get; set; }

    public DateTime ExpiresAtUtc => CreatedAt + JoinWindow;
    public bool IsJoinExpired => DateTime.UtcNow >= ExpiresAtUtc;

    public bool IsFull => AllPlayerIds.Count >= MaxPlayers;
    public IReadOnlyList<string> AllPlayerIds => Teams.SelectMany(t => t.Players).ToList();
    /// <summary>
    /// Lock used to serialize mutations of <see cref="Teams"/>, <see cref="Status"/>,
    /// and <see cref="LaunchedGameId"/>. Critical sections MUST be synchronous (no
    /// awaits inside the lock block) and as short as possible. Never lock more than
    /// one room's <see cref="SyncRoot"/> in the same call stack.
    /// </summary>
    public object SyncRoot => _syncRoot;

    public GameRoom(
        Guid roomId,
        int minigameType,
        string hostPlayerId,
        int maxPlayers,
        List<GameRoomTeam> teams,
        Dictionary<string, object>? gameParameters)
    {
        RoomId = roomId;
        MinigameType = minigameType;
        HostPlayerId = hostPlayerId;
        MaxPlayers = maxPlayers;
        Teams = teams;
        GameParameters = gameParameters;
    }

    public bool ContainsPlayer(string playerId) => Teams.Any(t => t.ContainsPlayer(playerId));

    public int? GetPlayerTeamIndex(string playerId) =>
        Teams.FirstOrDefault(t => t.ContainsPlayer(playerId))?.TeamIndex;
}
