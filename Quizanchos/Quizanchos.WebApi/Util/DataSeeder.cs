using Microsoft.AspNetCore.Identity;

namespace Quizanchos.WebApi.Util;

public class DataSeeder
{
    public static async Task SeedRoles(IServiceProvider serviceProvider)
    {
        RoleManager<IdentityRole> roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        string[] roleNames = { "Admin", "User" };
        IdentityResult roleResult;

        foreach (var roleName in roleNames)
        {
            bool roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }
}
