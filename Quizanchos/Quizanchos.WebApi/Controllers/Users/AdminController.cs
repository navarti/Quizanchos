using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizanchos.WebApi.Constants;
using Quizanchos.WebApi.Dto;
using Quizanchos.WebApi.Dto.Statistics;
using Quizanchos.WebApi.Services.Users;

namespace Quizanchos.WebApi.Controllers.Users;

[Route("[controller]/[action]")]
[Authorize(AppRole.Admin)]
public class AdminController : Controller
{
    private readonly AdminService _adminService;
    private readonly StatisticsService _statisticsService;

    public AdminController(AdminService adminService, StatisticsService statisticsService)
    {
        _adminService = adminService;
        _statisticsService = statisticsService;
    }

    [HttpPost]
    public async Task<IActionResult> DeleteUser([FromBody] string email)
    {
        await _adminService.DeleteUser(email);
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers(string name, int take, int skip)
    {
        IEnumerable<AdminUserDto> users = await _adminService.GetUsersAsync(name, take, skip);
        return Ok(users);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateUserNickname([FromBody] UpdateUserNicknameDto dto)
    {
        await _adminService.UpdateUserNickname(dto.Email, dto.NewNickname);
        return Ok();
    }

    [HttpGet]
    public IActionResult GetStatisticsGames()
    {
        var games = _statisticsService.GetRegisteredGames();
        return Ok(games);
    }

    [HttpGet]
    public async Task<IActionResult> GetStatisticsOverview([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var (fromUtc, toUtc) = ResolveRange(from, to);
        var overview = await _statisticsService.GetOverviewAsync(fromUtc, toUtc);
        return Ok(overview);
    }

    [HttpGet]
    public async Task<IActionResult> GetStatisticsByGame([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var (fromUtc, toUtc) = ResolveRange(from, to);
        var rows = await _statisticsService.GetSessionsByGameAsync(fromUtc, toUtc);
        return Ok(rows);
    }

    [HttpGet]
    public async Task<IActionResult> GetStatisticsTimeSeries(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? bucket,
        [FromQuery] int? minigameType)
    {
        var (fromUtc, toUtc) = ResolveRange(from, to);
        var bucketKind = ParseBucket(bucket, fromUtc, toUtc);
        var series = await _statisticsService.GetSessionsTimeSeriesAsync(fromUtc, toUtc, bucketKind, minigameType);
        return Ok(series);
    }

    [HttpGet]
    public async Task<IActionResult> GetStatisticsTopPlayers(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? limit)
    {
        var (fromUtc, toUtc) = ResolveRange(from, to);
        var players = await _statisticsService.GetTopPlayersAsync(fromUtc, toUtc, limit ?? 10);
        return Ok(players);
    }

    private static (DateTime FromUtc, DateTime ToUtc) ResolveRange(DateTime? from, DateTime? to)
    {
        var nowUtc = DateTime.UtcNow;
        var toUtc = (to ?? nowUtc).ToUniversalTime();
        var fromUtc = (from ?? toUtc.AddDays(-30)).ToUniversalTime();
        return (fromUtc, toUtc);
    }

    private static StatisticsService.BucketKind ParseBucket(string? bucket, DateTime fromUtc, DateTime toUtc)
    {
        if (Enum.TryParse<StatisticsService.BucketKind>(bucket, ignoreCase: true, out var parsed))
        {
            return parsed;
        }

        // Auto-pick a sane bucket if the client didn't specify one.
        var span = toUtc - fromUtc;
        if (span <= TimeSpan.FromHours(48)) return StatisticsService.BucketKind.Hour;
        if (span <= TimeSpan.FromDays(60)) return StatisticsService.BucketKind.Day;
        if (span <= TimeSpan.FromDays(180)) return StatisticsService.BucketKind.Week;
        return StatisticsService.BucketKind.Month;
    }
}

public record UpdateUserNicknameDto(string Email, string NewNickname);
