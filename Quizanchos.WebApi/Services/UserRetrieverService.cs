using Microsoft.AspNetCore.Identity;
using Quizanchos.Common.Util;
using Quizanchos.Domain.Entities;
using System.Security.Claims;

namespace Quizanchos.WebApi.Services;

public class UserRetrieverService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserRetrieverService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public string GetUserId(ClaimsPrincipal claimsPrincipal)
    {
        string? userId = _userManager.GetUserId(claimsPrincipal);
        return userId ?? throw HandledExceptionFactory.CreateNullException(nameof(userId));
    }

    public async Task<ApplicationUser> GetUserByClaims(ClaimsPrincipal claimsPrincipal)
    {
        return await _userManager.GetUserAsync(claimsPrincipal) ?? throw HandledExceptionFactory.CreateNullException(nameof(ApplicationUser));
    }
}
