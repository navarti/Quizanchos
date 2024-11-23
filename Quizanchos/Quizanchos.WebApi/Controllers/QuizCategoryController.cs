using Microsoft.AspNetCore.Authorization;
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
    [Authorize(QuizPolicy.Admin)]
    public async Task<IActionResult> Create([FromBody] BaseQuizCategoryDto baseQuizCategoryDto)
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
    public async Task<IActionResult> GetAll()
    {
        List<QuizCategoryDto> quizCategoryDtos = await _quizCategoryService.GetAll();
        return Ok(quizCategoryDtos);
    }
    
    #region Test purposes
#if DEBUG
    [HttpGet]
    [Authorize(QuizPolicy.User)]
    public async Task<IActionResult> TestGetAllByUser()
    {
        return await GetAll();
    }

    [HttpGet]
    [Authorize(QuizPolicy.Admin)]
    public async Task<IActionResult> TestGetAllByAdmin()
    {
        return await GetAll();
    }

    [HttpGet]
    [Authorize(QuizPolicy.Owner)]
    public async Task<IActionResult> TestGetAllByOwner()
    {
        return await GetAll();
    }
#endif
    #endregion

    [HttpPost]
    [Authorize(QuizPolicy.Admin)]
    public async Task<IActionResult> Update([FromBody] QuizCategoryDto quizCategoryDto)
    {
        QuizCategoryDto updatedQuizCategoryDto = await _quizCategoryService.Update(quizCategoryDto);
        return Ok(updatedQuizCategoryDto);
    }

    [HttpDelete]
    [Authorize(QuizPolicy.Admin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _quizCategoryService.Delete(id);
        return NoContent();
    }
}
