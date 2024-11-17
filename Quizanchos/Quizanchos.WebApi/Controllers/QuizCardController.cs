using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizanchos.WebApi.Constants;
using Quizanchos.WebApi.Dto;
using Quizanchos.WebApi.Dto.Abstractions;
using Quizanchos.WebApi.Services;

namespace Quizanchos.WebApi.Controllers;

[Route("[controller]/[action]")]
public class QuizCardController : Controller
{
    private readonly MainQuizCardService _quizCardService;
    private readonly SingleGameSessionService _singleGameSessionService;

    public QuizCardController(MainQuizCardService quizCardService, SingleGameSessionService singleGameSessionService)
    {
        _quizCardService = quizCardService;
        _singleGameSessionService = singleGameSessionService;
    }

    [HttpGet]
    [Authorize(QuizPolicy.User)]
    public async Task<IActionResult> GetCardForSession(Guid sessionId, int cardIndex)
    {
        SingleGameSessionDto session = await _singleGameSessionService.GetById(sessionId);
        QuizCardDtoAbstract card = await _quizCardService.GetCardForSession(User, sessionId, cardIndex);
        return Ok(card);
    }
}
