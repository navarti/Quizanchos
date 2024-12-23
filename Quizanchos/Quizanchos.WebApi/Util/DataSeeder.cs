﻿using Microsoft.AspNetCore.Identity;
using Quizanchos.Domain.Entities;
using Quizanchos.WebApi.Constants;

namespace Quizanchos.WebApi.Util;

public static class DataSeeder
{
    public static async Task SeedDatabase(IServiceProvider serviceProvider, ConfigurationManager configuration)
    {
        await SeedRoles(serviceProvider);
        await SeedOwner(serviceProvider, configuration);
    }

    public static async Task SeedRoles(IServiceProvider serviceProvider)
    {
        RoleManager<IdentityRole> roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        IdentityResult roleResult;

        foreach (var roleName in QuizRole.All)
        {
            bool roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }

    // Only one owner is allowed
    public static async Task SeedOwner(IServiceProvider serviceProvider, ConfigurationManager configuration)
    {
        string ownerEmail = configuration.GetOption("Owner:Email");
        string ownerPassword = configuration.GetOption("Owner:Password");

        UserManager<ApplicationUser> userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (ApplicationUser ownerToDelete in await userManager.GetUsersInRoleAsync(QuizRole.Owner))
        {
            await userManager.DeleteAsync(ownerToDelete);
        }

        ApplicationUser? owner = await userManager.FindByEmailAsync(ownerEmail);
        if (owner is not null)
        {
            throw CriticalExceptionFactory.Create($"{ownerEmail} can not be used for owner. It is already taken.");
        }

        owner = new ApplicationUser
        {
            UserName = ownerEmail,
            Email = ownerEmail,
        };

        IdentityResult result = await userManager.CreateAsync(owner, ownerPassword);
        if (!result.Succeeded)
        {
            throw CriticalExceptionFactory.CreateIdentityResultException(result);
        }

        result = await userManager.AddToRoleAsync(owner, QuizRole.Owner);
        if (!result.Succeeded)
        {
            throw CriticalExceptionFactory.CreateIdentityResultException(result);
        }
    }
}
