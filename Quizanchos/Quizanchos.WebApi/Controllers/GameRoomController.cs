using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizanchos.WebApi.Constants;
using Quizanchos.WebApi.Models.Rooms;
using Quizanchos.WebApi.Services.Rooms;
using System.Security.Claims;

namespace Quizanchos.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GameRoomController : ControllerBase
{
    private readonly GameRoomService _roomService;

    public GameRoomController(GameRoomService roomService)
    {
        _roomService = roomService;
    }

    /// <summary>
    /// Create a new game room. The caller becomes the host and is auto-joined to team 0.
    /// </summary>
    [HttpPost]
    [Authorize(AppRole.User)]
    public async Task<IActionResult> CreateRoom([FromBody] CreateRoomRequest request)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        RoomActionResult result = await _roomService.CreateRoomAsync(request, userId);

        if (!result.IsSuccess)
            return BadRequest(new { Message = result.ErrorMessage });

        return Ok(result.Room);
    }

    /// <summary>
    /// List all rooms currently waiting for players, optionally filtered by game type.
    /// </summary>
    [HttpGet]
    [Authorize(AppRole.User)]
    public IActionResult GetAvailableRooms([FromQuery] int? minigameType)
    {
        var rooms = _roomService.GetAvailableRooms(minigameType);
        return Ok(rooms);
    }

    /// <summary>
    /// Get details of a specific room.
    /// </summary>
    [HttpGet("{roomId}")]
    [Authorize(AppRole.User)]
    public IActionResult GetRoom(Guid roomId)
    {
        RoomActionResult result = _roomService.GetRoom(roomId);

        if (!result.IsSuccess)
            return NotFound(new { Message = result.ErrorMessage });

        return Ok(result.Room);
    }

    /// <summary>
    /// Join a specific team in a room. When the room becomes full the game is auto-launched.
    /// </summary>
    [HttpPost("{roomId}/join")]
    [Authorize(AppRole.User)]
    public async Task<IActionResult> JoinRoom(Guid roomId, [FromBody] JoinRoomRequest request)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        RoomActionResult result = await _roomService.JoinRoomAsync(roomId, request.TeamIndex, userId);

        if (!result.IsSuccess)
            return BadRequest(new { Message = result.ErrorMessage });

        return Ok(result.Room);
    }

    /// <summary>
    /// Leave a room. If the host leaves, the room is closed for all participants.
    /// </summary>
    [HttpPost("{roomId}/leave")]
    [Authorize(AppRole.User)]
    public async Task<IActionResult> LeaveRoom(Guid roomId)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        RoomActionResult result = await _roomService.LeaveRoomAsync(roomId, userId);

        if (!result.IsSuccess)
            return BadRequest(new { Message = result.ErrorMessage });

        return Ok(result.Room);
    }

    /// <summary>
    /// Switch to a different team within the same room.
    /// </summary>
    [HttpPost("{roomId}/switch-team")]
    [Authorize(AppRole.User)]
    public async Task<IActionResult> SwitchTeam(Guid roomId, [FromBody] SwitchTeamRequest request)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        RoomActionResult result = await _roomService.SwitchTeamAsync(roomId, request.NewTeamIndex, userId);

        if (!result.IsSuccess)
            return BadRequest(new { Message = result.ErrorMessage });

        return Ok(result.Room);
    }
}
