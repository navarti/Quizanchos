﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizanchos.WebApi.Constants;
using Quizanchos.WebApi.Dto;
using Quizanchos.WebApi.Services;

namespace Quizanchos.WebApi.Controllers;

[Route("[controller]/[action]")]
[Authorize(QuizPolicy.Admin)]
public class AdminController : Controller
{
    private readonly AdminService _adminService;

    public AdminController(AdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpPost]
    public async Task<IActionResult> DeleteUser([FromBody] string email)
    {
        await _adminService.DeleteUser(email);
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers(string name, int take, int skip)
    {
        IEnumerable<ApplicationUserDto> users = await _adminService.GetUsersAsync(name, take, skip);
        return Ok(users);
    }
}
