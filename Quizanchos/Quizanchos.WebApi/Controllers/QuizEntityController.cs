using Microsoft.AspNetCore.Mvc;
using Quizanchos.WebApi.Dto;
using Quizanchos.WebApi.Services.Interfaces;

namespace Quizanchos.WebApi.Controllers;

[Route("[controller]/[action]")]
public class QuizEntityController : Controller
{
    private readonly IQuizEntityService _quizEntityService;

    public QuizEntityController(IQuizEntityService quizEntityService)
    {
        _quizEntityService = quizEntityService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(BaseQuizEntityDto baseQuizEntityDto)
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
    public async Task<IActionResult> Update(QuizEntityDto quizEntityDto)
    {
        QuizEntityDto updatedQuizEntityDto = await _quizEntityService.Update(quizEntityDto);
        return Ok(updatedQuizEntityDto);
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _quizEntityService.Delete(id);
        return Ok();
    }
}
