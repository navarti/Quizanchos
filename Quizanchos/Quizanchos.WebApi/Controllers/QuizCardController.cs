using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizanchos.WebApi.Constants;
using Quizanchos.WebApi.Services;

namespace Quizanchos.WebApi.Controllers;

[Route("[controller]/[action]")]
public class QuizCardController : Controller
{
    private readonly MainQuizCardService _quizCardService;

    public QuizCardController(MainQuizCardService quizCardService)
    {
        _quizCardService = quizCardService;
    }

    [HttpGet]
    [Authorize(QuizPolicy.User)]
    public async Task<IActionResult> GetCardForSession(Guid sessionId, int cardIndex)
    {
        Dto.Abstractions.QuizCardDtoAbstract res = await _quizCardService.GetCardForSession(User, sessionId, cardIndex);
        return Ok(res);
    }
}
