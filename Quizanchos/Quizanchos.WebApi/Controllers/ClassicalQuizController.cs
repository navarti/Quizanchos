﻿using Microsoft.AspNetCore.Mvc;
using Quizanchos.WebApi.Services;

namespace Quizanchos.WebApi.Controllers;

[Route("[controller]/[action]")]
public class ClassicalQuizController : Controller
{
    private readonly ClassicalQuizService _classicalQuizService;

    public ClassicalQuizController(ClassicalQuizService classicalQuizService)
    {
        _classicalQuizService = classicalQuizService;
    }

    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpGet]
    public async Task<IActionResult> Test()
    {
        return Ok("Server is running");
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return View();
    }
}
