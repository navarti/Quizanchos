using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Quizanchos.WebApi.Dto;
using Quizanchos.WebApi.Services;

namespace Quizanchos.WebApi.Controllers;

[Route("[controller]/[action]")]
public class ContactController : Controller
{
    private readonly ContactService _contactService;

    public ContactController(ContactService contactService)
    {
        _contactService = contactService;
    }

    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("contact")]
    public async Task<IActionResult> Submit([FromBody] ContactMessageDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { Message = "Please fill in all fields correctly." });
        }

        await _contactService.SubmitMessageAsync(dto, User);
        return Ok();
    }
}
