using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizanchos.WebApi.Constants;
using Quizanchos.WebApi.Dto;
using Quizanchos.WebApi.Services;

namespace Quizanchos.WebApi.Controllers;

[Route("[controller]/[action]")]
public class QuizEntityController : Controller
{
    private readonly QuizEntityService _quizEntityService;

    public QuizEntityController(QuizEntityService quizEntityService)
    {
        _quizEntityService = quizEntityService;
    }

    [HttpPost]
    [Authorize(QuizPolicy.Admin)]
    public async Task<IActionResult> Create([FromBody] BaseQuizEntityDto baseQuizEntityDto)
    {
        QuizEntityDto quizEntityDto = await _quizEntityService.Create(baseQuizEntityDto);
        return Ok(quizEntityDto);
    }

    [HttpGet]
    public async Task<IActionResult> GetById(Guid id)
    {
        QuizEntityDto quizEntityDto = await _quizEntityService.GetById(id);
        return Ok(quizEntityDto);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        List<QuizEntityDto> quizEntityDtos = await _quizEntityService.GetAll();
        return Ok(quizEntityDtos);
    }

    [HttpPost]
    [Authorize(QuizPolicy.Admin)]
    public async Task<IActionResult> Update([FromBody] QuizEntityDto quizEntityDto)
    {
        QuizEntityDto updatedQuizEntityDto = await _quizEntityService.Update(quizEntityDto);
        return Ok(updatedQuizEntityDto);
    }

    [HttpDelete]
    [Authorize(QuizPolicy.Admin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _quizEntityService.Delete(id);
        return NoContent();
    }
}
