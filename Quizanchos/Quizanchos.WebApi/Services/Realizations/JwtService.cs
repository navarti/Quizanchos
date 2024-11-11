﻿using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Quizanchos.Domain.Entities;
using Quizanchos.WebApi.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Quizanchos.WebApi.Services.Realizations;

public class JwtService : IJwtService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration, UserManager<ApplicationUser> userManager)
    {
        _configuration = configuration;
        _userManager = userManager;
    }

    public async Task<string> GenerateAcessTokenAsync(ApplicationUser user)
    {
        JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

        byte[] privateKey = Encoding.UTF8.GetBytes(_configuration["JWT:Secret"] ?? "");

        SigningCredentials credentials = new SigningCredentials(
            new SymmetricSecurityKey(privateKey),
            SecurityAlgorithms.HmacSha256);

        SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
        {
            SigningCredentials = credentials,
            Expires = GetExpiryDate(),
            Subject = await GenerateClaims(user)
        };

        SecurityToken token = handler.CreateToken(tokenDescriptor);
        return handler.WriteToken(token);
    }

    private async Task<ClaimsIdentity> GenerateClaims(ApplicationUser user)
    {
        ClaimsIdentity ci = new ClaimsIdentity();

        ci.AddClaim(new Claim(ClaimTypes.Name, user.UserName));

        IList<string> userRoles = await _userManager.GetRolesAsync(user);
        foreach (var role in userRoles)
            ci.AddClaim(new Claim(ClaimTypes.Role, role));
        
        return ci;
    }

    private DateTime GetExpiryDate()
    {
        _ = int.TryParse(_configuration["JWT:AccessTokenValidityInMinutes"], out int tokenValidityInMinutes);
        return DateTime.UtcNow.AddMinutes(tokenValidityInMinutes);
    }
}