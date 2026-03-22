using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Quizanchos.Domain;
using Quizanchos.Domain.Entities;
using Quizanchos.WebApi.Constants;

namespace Quizanchos.WebApi.Util;

public static class DataSeeder
{
    public static async Task SeedDatabase(IServiceProvider serviceProvider, ConfigurationManager configuration)
    {
        await SeedRoles(serviceProvider);
        await SeedOwner(serviceProvider, configuration);
        await SeedMarketItems(serviceProvider);
    }

    public static async Task SeedRoles(IServiceProvider serviceProvider)
    {
        RoleManager<IdentityRole> roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        IdentityResult roleResult;

        foreach (var roleName in AppRole.All)
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

        foreach (ApplicationUser ownerToDelete in await userManager.GetUsersInRoleAsync(AppRole.Owner))
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

        result = await userManager.AddToRoleAsync(owner, AppRole.Owner);
        if (!result.Succeeded)
        {
            throw CriticalExceptionFactory.CreateIdentityResultException(result);
        }
    }

    public static async Task SeedMarketItems(IServiceProvider serviceProvider)
    {
        QuizanchosDbContext dbContext = serviceProvider.GetRequiredService<QuizanchosDbContext>();

        bool hasEmojiCatalog = await dbContext.MarketItems
            .AnyAsync(x => x.Type == MarketItemType.Emoji)
            .ConfigureAwait(false);

        if (hasEmojiCatalog)
        {
            return;
        }

        var emojiCatalog = new List<MarketItem>
        {
            new()
            {
                Id = Guid.Parse("7D11A1F8-9BE0-4B2B-9A03-196BAA4B5231"),
                Type = MarketItemType.Emoji,
                Name = "Thumbs Up",
                ImageUrl = "https://twemoji.maxcdn.com/v/latest/svg/1f44d.svg",
                PriceCoins = 0,
                IsFree = true,
                IsActive = true
            },
            new()
            {
                Id = Guid.Parse("E5E8229A-5B86-470C-BEAF-86B3A7A89E40"),
                Type = MarketItemType.Emoji,
                Name = "Fire",
                ImageUrl = "https://twemoji.maxcdn.com/v/latest/svg/1f525.svg",
                PriceCoins = 120,
                IsFree = false,
                IsActive = true
            },
            new()
            {
                Id = Guid.Parse("90B2A880-FFB8-4A84-A2D7-5A8F3797B5D4"),
                Type = MarketItemType.Emoji,
                Name = "Party",
                ImageUrl = "https://twemoji.maxcdn.com/v/latest/svg/1f389.svg",
                PriceCoins = 200,
                IsFree = false,
                IsActive = true
            },
            new()
            {
                Id = Guid.Parse("90CAC16A-29CF-47AF-A713-1695D16F27C4"),
                Type = MarketItemType.Emoji,
                Name = "Trophy",
                ImageUrl = "https://twemoji.maxcdn.com/v/latest/svg/1f3c6.svg",
                PriceCoins = 300,
                IsFree = false,
                IsActive = true
            }
        };

        await dbContext.MarketItems.AddRangeAsync(emojiCatalog).ConfigureAwait(false);
        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
}
