using Microsoft.AspNetCore.Identity;
using Quizanchos.WebApi.Constants;

namespace Quizanchos.WebApi.Util;

public class DataSeeder
{
    public static async Task SeedRoles(IServiceProvider serviceProvider)
    {
        RoleManager<IdentityRole> roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        IdentityResult roleResult;

        foreach (var roleName in Role.All)
        {
            bool roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }
}
