using Microsoft.AspNetCore.Identity;
using Quizanchos.Core;
using Quizanchos.Domain.Entities;
using Quizanchos.WebApi.Models.Rooms;
using Quizanchos.WebApi.Services.Users;

namespace Quizanchos.WebApi.Services.Rooms;

/// <summary>
/// Orchestrates the game-room lifecycle: create ? join / leave / switch team ? auto-launch.
/// Delegates persistence to <see cref="IGameRoomManager"/> and notifications to <see cref="IRoomNotifier"/>.
/// </summary>
public class GameRoomService
{
    private readonly IGameRoomManager _roomManager;
    private readonly IRoomNotifier _roomNotifier;
    private readonly GameService _gameService;
    private readonly ILogger<GameRoomService> _logger;
    private readonly PremiumAccessService _premiumAccessService;
    private readonly UserManager<ApplicationUser> _userManager;

    public GameRoomService(
        IGameRoomManager roomManager,
        IRoomNotifier roomNotifier,
        GameService gameService,
        ILogger<GameRoomService> logger,
        PremiumAccessService premiumAccessService,
        UserManager<ApplicationUser> userManager)
    {
        _roomManager = roomManager;
        _roomNotifier = roomNotifier;
        _gameService = gameService;
        _logger = logger;
        _premiumAccessService = premiumAccessService;
        _userManager = userManager;
    }

    public async Task<RoomActionResult> CreateRoomAsync(CreateRoomRequest request, string hostPlayerId)
    {
        await _premiumAccessService
            .EnsureUsersCanAccessMinigameAsync([hostPlayerId], request.MinigameType)
            .ConfigureAwait(false);

        var existingRoom = _roomManager.GetRoomByPlayerId(hostPlayerId);
        if (existingRoom != null)
            return RoomActionResult.Error($"Player is already in room {existingRoom.RoomId}");

        var activeSessionId = await _gameService.GetActiveSessionIdAsync(hostPlayerId);
        if (activeSessionId != null)
            return RoomActionResult.Error("You already have a game in progress. Finish or leave it before creating a room.");

        if (request.MaxPlayers < 2)
            return RoomActionResult.Error("Multiplayer rooms require at least 2 players");

        if (request.TeamCount < 1)
            return RoomActionResult.Error("At least 1 team is required");

        if (request.MaxPlayers < request.TeamCount)
            return RoomActionResult.Error("Max players cannot be less than team count");

        Guid roomId = Guid.NewGuid();
        int playersPerTeam = (int)Math.Ceiling((double)request.MaxPlayers / request.TeamCount);

        var teams = new List<GameRoomTeam>();
        for (int i = 0; i < request.TeamCount; i++)
        {
            teams.Add(new GameRoomTeam(i, $"Team {i + 1}", playersPerTeam));
        }

        var room = _roomManager.CreateRoom(
            roomId, request.MinigameType, hostPlayerId,
            request.MaxPlayers, teams, request.GameParameters);

        // Host automatically joins the first team
        lock (room.SyncRoot)
        {
            room.Teams[0].AddPlayer(hostPlayerId);
        }

        _logger.LogInformation(
            "Room created: RoomId={RoomId}, Host={HostId}, Type={Type}, MaxPlayers={Max}, Teams={Teams}",
            roomId, hostPlayerId, request.MinigameType, request.MaxPlayers, request.TeamCount);

        return RoomActionResult.Success(await MapToDtoAsync(room));
    }

    public async Task<RoomActionResult> JoinRoomAsync(Guid roomId, int teamIndex, string playerId)
    {
        var existingRoom = _roomManager.GetRoomByPlayerId(playerId);
        if (existingRoom != null && existingRoom.RoomId != roomId)
            return RoomActionResult.Error($"Player is already in room {existingRoom.RoomId}");

        var activeSessionId = await _gameService.GetActiveSessionIdAsync(playerId);
        if (activeSessionId != null)
            return RoomActionResult.Error("You already have a game in progress. Finish or leave it before joining a room.");

        var room = _roomManager.GetRoom(roomId);
        if (room == null)
            return RoomActionResult.Error("Room not found");

        await _premiumAccessService
            .EnsureUsersCanAccessMinigameAsync([playerId], room.MinigameType)
            .ConfigureAwait(false);

        bool shouldLaunch;

        lock (room.SyncRoot)
        {
            if (room.IsJoinExpired)
                return RoomActionResult.Error("Room has expired and is no longer accepting players");

            if (room.Status != GameRoomStatus.WaitingForPlayers)
                return RoomActionResult.Error("Room is no longer accepting players");

            if (room.ContainsPlayer(playerId))
                return RoomActionResult.Error("Player is already in this room");

            if (teamIndex < 0 || teamIndex >= room.Teams.Count)
                return RoomActionResult.Error($"Invalid team index: {teamIndex}");

            var team = room.Teams[teamIndex];
            if (!team.AddPlayer(playerId))
                return RoomActionResult.Error("Team is full");

            shouldLaunch = room.IsFull;
            if (shouldLaunch)
                room.Status = GameRoomStatus.Launching;
        }

        _logger.LogInformation("Player {PlayerId} joined room {RoomId} team {TeamIndex}",
            playerId, roomId, teamIndex);

        await _roomNotifier.NotifyPlayerJoinedRoom(roomId, playerId, teamIndex);
        await _roomNotifier.NotifyRoomUpdated(roomId, await MapToDtoAsync(room));

        if (shouldLaunch)
        {
            await LaunchGameFromRoomAsync(room);
        }

        return RoomActionResult.Success(await MapToDtoAsync(room));
    }

    public async Task<RoomActionResult> LeaveRoomAsync(Guid roomId, string playerId)
    {
        var room = _roomManager.GetRoom(roomId);
        if (room == null)
            return RoomActionResult.Error("Room not found");

        bool hostLeft;

        lock (room.SyncRoot)
        {
            if (room.Status != GameRoomStatus.WaitingForPlayers)
                return RoomActionResult.Error("Cannot leave room in current state");

            if (!room.ContainsPlayer(playerId))
                return RoomActionResult.Error("Player is not in this room");

            foreach (var team in room.Teams)
                team.RemovePlayer(playerId);

            hostLeft = playerId == room.HostPlayerId;
        }

        _logger.LogInformation("Player {PlayerId} left room {RoomId}", playerId, roomId);
        await _roomNotifier.NotifyPlayerLeftRoom(roomId, playerId);

        if (hostLeft)
        {
            await CloseRoomAsync(roomId, "Host left the room");
            return RoomActionResult.Success(await MapToDtoAsync(room));
        }

        await _roomNotifier.NotifyRoomUpdated(roomId, await MapToDtoAsync(room));
        return RoomActionResult.Success(await MapToDtoAsync(room));
    }

    public async Task<RoomActionResult> SwitchTeamAsync(Guid roomId, int newTeamIndex, string playerId)
    {
        var room = _roomManager.GetRoom(roomId);
        if (room == null)
            return RoomActionResult.Error("Room not found");

        lock (room.SyncRoot)
        {
            if (room.Status != GameRoomStatus.WaitingForPlayers)
                return RoomActionResult.Error("Cannot switch teams in current state");

            if (!room.ContainsPlayer(playerId))
                return RoomActionResult.Error("Player is not in this room");

            if (newTeamIndex < 0 || newTeamIndex >= room.Teams.Count)
                return RoomActionResult.Error($"Invalid team index: {newTeamIndex}");

            var newTeam = room.Teams[newTeamIndex];
            if (newTeam.IsFull)
                return RoomActionResult.Error("Target team is full");

            foreach (var team in room.Teams)
                team.RemovePlayer(playerId);

            newTeam.AddPlayer(playerId);
        }

        _logger.LogInformation("Player {PlayerId} switched to team {TeamIndex} in room {RoomId}",
            playerId, newTeamIndex, roomId);

        await _roomNotifier.NotifyRoomUpdated(roomId, await MapToDtoAsync(room));
        return RoomActionResult.Success(await MapToDtoAsync(room));
    }

    public async Task<RoomActionResult> GetRoomAsync(Guid roomId)
    {
        var room = _roomManager.GetRoom(roomId);
        if (room == null)
            return RoomActionResult.Error("Room not found");

        return RoomActionResult.Success(await MapToDtoAsync(room));
    }

    public async Task<IReadOnlyList<GameRoomDto>> GetAvailableRoomsAsync(int? minigameType = null)
    {
        var rooms = _roomManager.GetAvailableRooms(minigameType);
        var dtos = new List<GameRoomDto>(rooms.Count);
        foreach (var room in rooms)
        {
            dtos.Add(await MapToDtoAsync(room));
        }
        return dtos;
    }

    private async Task LaunchGameFromRoomAsync(GameRoom room)
    {
        _logger.LogInformation("Launching game from room {RoomId}", room.RoomId);

        try
        {
            var playerIds = room.AllPlayerIds.ToList();

            await _premiumAccessService
                .EnsureUsersCanAccessMinigameAsync(playerIds, room.MinigameType)
                .ConfigureAwait(false);

            var teams = room.Teams
                .Select(t => new TeamInfo(t.TeamIndex, t.Name, t.Players.ToList()))
                .ToList();

            IReadOnlyDictionary<string, string> nicknames =
                await ResolveNicknamesAsync(playerIds);

            var response = await _gameService.CreateMultiPlayerGameAsync(
                room.MinigameType,
                playerIds,
                room.GameParameters,
                teams,
                nicknames);

            lock (room.SyncRoot)
            {
                room.LaunchedGameId = response.GameId;
                room.Status = GameRoomStatus.GameStarted;
            }

            await _roomNotifier.NotifyGameStarting(room.RoomId, response.GameId);

            _logger.LogInformation("Game {GameId} launched from room {RoomId}",
                response.GameId, room.RoomId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to launch game from room {RoomId}", room.RoomId);

            lock (room.SyncRoot)
            {
                room.Status = GameRoomStatus.WaitingForPlayers;
            }

            await _roomNotifier.NotifyRoomUpdated(room.RoomId, await MapToDtoAsync(room));
        }
    }

    private async Task CloseRoomAsync(Guid roomId, string reason)
    {
        var room = _roomManager.GetRoom(roomId);
        if (room != null)
        {
            lock (room.SyncRoot)
            {
                room.Status = GameRoomStatus.Closed;
            }
        }

        await _roomNotifier.NotifyRoomClosed(roomId, reason);
        _roomManager.RemoveRoom(roomId);

        _logger.LogInformation("Room {RoomId} closed: {Reason}", roomId, reason);
    }

    internal async Task<GameRoomDto> MapToDtoAsync(GameRoom room)
    {
        IReadOnlyDictionary<string, string> nicknames =
            await ResolveNicknamesAsync(room.AllPlayerIds);

        return new GameRoomDto
        {
            RoomId = room.RoomId,
            MinigameType = room.MinigameType,
            HostPlayerId = room.HostPlayerId,
            MaxPlayers = room.MaxPlayers,
            CurrentPlayerCount = room.AllPlayerIds.Count,
            Status = room.Status,
            Teams = room.Teams.Select(t => new GameRoomTeamDto
            {
                TeamIndex = t.TeamIndex,
                Name = t.Name,
                MaxSize = t.MaxSize,
                Players = t.Players,
                IsFull = t.IsFull
            }).ToList(),
            PlayerNicknames = nicknames,
            CreatedAt = room.CreatedAt,
            ExpiresAtUtc = room.ExpiresAtUtc,
            LaunchedGameId = room.LaunchedGameId
        };
    }

    private async Task<IReadOnlyDictionary<string, string>> ResolveNicknamesAsync(
        IReadOnlyCollection<string> playerIds)
    {
        Dictionary<string, string> result = new(playerIds.Count);
        foreach (string playerId in playerIds.Distinct())
        {
            ApplicationUser? user = await _userManager.FindByIdAsync(playerId);
            result[playerId] = user?.UserName ?? playerId;
        }
        return result;
    }
}
