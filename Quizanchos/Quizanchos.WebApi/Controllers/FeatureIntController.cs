using Microsoft.AspNetCore.Mvc;
using Quizanchos.WebApi.Dto;
using Quizanchos.WebApi.Services;

namespace Quizanchos.WebApi.Controllers;

[Route("[controller]/[action]")]
public class FeatureIntController : Controller
{
    private readonly FeatureIntService _featureIntService;

    public FeatureIntController(FeatureIntService featureIntService)
    {
        _featureIntService = featureIntService;
    }

    [HttpGet]
    public async Task<IActionResult> GetById(Guid id)
    {
        FeatureIntDto feature = await _featureIntService.GetById(id);
        return Ok(feature);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllByCategory(Guid categoryId)
    {
        List<FeatureIntDto> features = await _featureIntService.GetAllByCategory(categoryId);
        return Ok(features);
    }

    [HttpPost]
    public async Task<IActionResult> Create(BaseFeatureIntDto baseFeatureIntDto)
    {
        FeatureIntDto feature = await _featureIntService.Create(baseFeatureIntDto);
        return Ok(feature);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _featureIntService.Delete(id);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Update(FeatureIntDto featureIntDto)
    {
        featureIntDto = await _featureIntService.Update(featureIntDto);
        return Ok(featureIntDto);
    }
}
