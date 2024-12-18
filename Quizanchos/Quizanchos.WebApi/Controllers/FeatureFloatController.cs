using Microsoft.AspNetCore.Mvc;
using Quizanchos.WebApi.Dto;
using Quizanchos.WebApi.Services;

namespace Quizanchos.WebApi.Controllers;

[Route("[controller]/[action]")]
public class FeatureFloatController : Controller
{
    private readonly FeatureFloatService _featureFloatService;

    public FeatureFloatController(FeatureFloatService featureFloatService)
    {
        _featureFloatService = featureFloatService;
    }

    [HttpGet]
    public async Task<IActionResult> GetById(Guid id)
    {
        FeatureFloatDto feature = await _featureFloatService.GetById(id);
        return Ok(feature);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllByCategory(Guid categoryId)
    {
        List<FeatureFloatDto> features = await _featureFloatService.GetAllByCategory(categoryId);
        return Ok(features);
    }

    [HttpPost]
    public async Task<IActionResult> Create(BaseFeatureFloatDto baseFeatureIntDto)
    {
        FeatureFloatDto feature = await _featureFloatService.Create(baseFeatureIntDto);
        return Ok(feature);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _featureFloatService.Delete(id);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Update([FromBody] FeatureFloatDto featurefloatDto)
    {
        featurefloatDto = await _featureFloatService.Update(featurefloatDto);
        return Ok(featurefloatDto);
    }
}
