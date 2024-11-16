using Microsoft.AspNetCore.Mvc;
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
}
