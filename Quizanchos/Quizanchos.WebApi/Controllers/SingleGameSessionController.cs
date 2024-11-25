using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizanchos.Common.Enums;
using Quizanchos.WebApi.Constants;
using Quizanchos.WebApi.Dto;
using Quizanchos.WebApi.Dto.Abstractions;
using Quizanchos.WebApi.Services;

namespace Quizanchos.WebApi.Controllers;

[Route("[controller]/[action]")]
public class SingleGameSessionController : Controller
{
    private readonly SingleGameSessionService _singleGameSessionService;

    public SingleGameSessionController(SingleGameSessionService singleGameSessionService)
    {
        _singleGameSessionService = singleGameSessionService;
    }

    [HttpPost]
    [Authorize(QuizPolicy.User)]
    public async Task<IActionResult> Create([FromBody] BaseSingleGameSessionDto baseSingleGameSessionDto)
    {
        if (baseSingleGameSessionDto == null)
        {
            return BadRequest(new { message = "Request body is null or malformed." });
        }

        if (baseSingleGameSessionDto.QuizCategoryId == Guid.Empty)
        {
            return BadRequest(new { message = "QuizCategoryId is invalid or missing." });
        }

        if (!Enum.IsDefined(typeof(GameLevel), baseSingleGameSessionDto.GameLevel))
        {
            return BadRequest(new { message = "GameLevel is invalid or missing." });
        }

        Console.WriteLine($"QuizCategoryId: {baseSingleGameSessionDto.QuizCategoryId}");
        Console.WriteLine($"GameLevel: {baseSingleGameSessionDto.GameLevel}");

        SingleGameSessionDto singleGameSession = await _singleGameSessionService.Create(baseSingleGameSessionDto, User);
        return Ok(singleGameSession);
    }

    [HttpGet]
    [Authorize(QuizPolicy.User)]
    public async Task<IActionResult> GetById(Guid sessionId)
    {
        SingleGameSessionDto singleGameSession = await _singleGameSessionService.GetById(User, sessionId);
        return Ok(singleGameSession);
    }

    [HttpGet]
    [Authorize(QuizPolicy.User)]
    public async Task<IActionResult> GetAliveSession()
    {
        SingleGameSessionDto? singleGameSession = await _singleGameSessionService.FindAliveSession(User);
        if (singleGameSession is null)
        {
            return NotFound();
        }
        return Ok(singleGameSession);
    }

    [HttpGet]
    [Authorize(QuizPolicy.User)]
    public async Task<IActionResult> GetCurrentCardForSession(Guid sessionId)
    {
        QuizCardDtoAbstract card = await _singleGameSessionService.GetCurrentCardForSession(User, sessionId);
        return Ok(card);
    }

    [HttpGet]
    [Authorize(QuizPolicy.User)]
    public async Task<IActionResult> GetCardForSession(Guid sessionId, int cardIndex)
    {
        QuizCardDtoAbstract card = await _singleGameSessionService.GetCardForSession(User, sessionId, cardIndex);
        return Ok(card);
    }

    [HttpPost]
    [Authorize(QuizPolicy.User)]
    public async Task<IActionResult> CreateNextCardForSession(Guid sessionId)
    {
        QuizCardDtoAbstract card = await _singleGameSessionService.CreateNextCardForSession(User, sessionId);
        return Ok(card);
    }

    [HttpPost]
    [Authorize(QuizPolicy.User)]
    public async Task<IActionResult> PickAnswerForSession(AnswerDto answerDto)
    {
        QuizCardDtoAbstract card = await _singleGameSessionService.PickAnswerForSession(User, answerDto);
        return Ok(card);
    }
    [HttpGet]
    [Authorize(QuizPolicy.User)]
    public async Task<IActionResult> GetBYid(Guid sessionId)
    {
        var card = await _singleGameSessionService.GetById(sessionId);
        return Ok(card);
    }
}
