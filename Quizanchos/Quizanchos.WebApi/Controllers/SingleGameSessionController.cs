﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizanchos.WebApi.Constants;
using Quizanchos.WebApi.Dto;
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
        SingleGameSessionDto singleGameSession = await _singleGameSessionService.Create(baseSingleGameSessionDto, User);
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
}
