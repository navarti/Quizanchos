﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizanchos.WebApi.Dto;
using Quizanchos.WebApi.Constants;
using Quizanchos.WebApi.Services;

namespace Quizanchos.WebApi.Controllers;

[Route("[controller]/[action]")]
public class QuizCategoryController : Controller
{
    private readonly QuizCategoryService _quizCategoryService;

    public QuizCategoryController(QuizCategoryService quizCategoryService)
    {
        _quizCategoryService = quizCategoryService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(BaseQuizCategoryDto baseQuizCategoryDto)
    {
        QuizCategoryDto quizCategoryDto = await _quizCategoryService.Create(baseQuizCategoryDto);
        return Ok(quizCategoryDto);
    }

    [HttpGet]
    public async Task<IActionResult> GetById(Guid id)
    {
        QuizCategoryDto quizCategoryDto = await _quizCategoryService.GetById(id);
        return Ok(quizCategoryDto);
    }

    [HttpGet]
    [Authorize(Role.User)]
    public async Task<IActionResult> GetAll()
    {
        List<QuizCategoryDto> quizCategoryDtos = await _quizCategoryService.GetAll();
        return Ok(quizCategoryDtos);
    }

    [HttpPost]
    public async Task<IActionResult> Update(QuizCategoryDto quizCategoryDto)
    {
        QuizCategoryDto updatedQuizCategoryDto = await _quizCategoryService.Update(quizCategoryDto);
        return Ok(updatedQuizCategoryDto);
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _quizCategoryService.Delete(id);
        return NoContent();
    }
}
