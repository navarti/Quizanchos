using Microsoft.AspNetCore.Mvc;
using Quizanchos.Quiz.Services;

namespace Quizanchos.WebApi.Controllers.Quiz;

[Route("[controller]/[action]")]
public class QuizCardController : Controller
{
    private readonly MainQuizCardService _quizCardService;

    public QuizCardController(MainQuizCardService quizCardService)
    {
        _quizCardService = quizCardService;
    }
}
