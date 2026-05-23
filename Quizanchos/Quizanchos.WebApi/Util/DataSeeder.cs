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
        await SeedTestUsers(serviceProvider);
    }

    public static async Task SeedTestUsers(IServiceProvider serviceProvider)
    {
        UserManager<ApplicationUser> userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var testUsers = new (string Email, string Password)[]
        {
            ("alice@test.com", "alice123"),
            ("bob@test.com", "bob123"),
        };

        foreach (var (email, password) in testUsers)
        {
            ApplicationUser? existing = await userManager.FindByEmailAsync(email);
            if (existing is not null)
            {
                continue;
            }

            ApplicationUser user = new()
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
            };
            IdentityResult create = await userManager.CreateAsync(user, password);
            if (!create.Succeeded)
            {
                continue;
            }
            await userManager.AddToRoleAsync(user, AppRole.User);
        }
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

        // Demote anyone holding the Owner role whose email doesn't match config.
        // Never delete — the user may own rows (top-up orders, sessions, etc.) with
        // restricted FKs back to AspNetUsers.
        foreach (ApplicationUser staleOwner in await userManager.GetUsersInRoleAsync(AppRole.Owner))
        {
            if (string.Equals(staleOwner.Email, ownerEmail, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            IdentityResult demote = await userManager.RemoveFromRoleAsync(staleOwner, AppRole.Owner);
            if (!demote.Succeeded)
            {
                throw CriticalExceptionFactory.CreateIdentityResultException(demote);
            }
        }

        ApplicationUser? owner = await userManager.FindByEmailAsync(ownerEmail);
        if (owner is not null)
        {
            if (!await userManager.IsInRoleAsync(owner, AppRole.Owner))
            {
                IdentityResult promote = await userManager.AddToRoleAsync(owner, AppRole.Owner);
                if (!promote.Succeeded)
                {
                    throw CriticalExceptionFactory.CreateIdentityResultException(promote);
                }
            }
            return;
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
        bool hasPremiumCatalog = await dbContext.MarketItems
            .AnyAsync(x => x.Type == MarketItemType.PremiumSubscription)
            .ConfigureAwait(false);

        var itemsToSeed = new List<MarketItem>();

        if (!hasEmojiCatalog)
        {
            itemsToSeed.AddRange(
            [
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
            ]);
        }

        if (!hasPremiumCatalog)
        {
            itemsToSeed.AddRange(
            [
                new()
                {
                    Id = Guid.Parse("3C0F2EC0-6A3A-4A2A-9F4F-B2D9E57204A1"),
                    Type = MarketItemType.PremiumSubscription,
                    Name = "Premium 1 Month",
                    ImageUrl = "https://twemoji.maxcdn.com/v/latest/svg/2b50.svg",
                    PriceCoins = 500,
                    IsFree = false,
                    DurationMonths = 1,
                    IsActive = true
                },
                new()
                {
                    Id = Guid.Parse("6D86BE5B-6B03-4BB9-8F8E-8B9D36AB69A3"),
                    Type = MarketItemType.PremiumSubscription,
                    Name = "Premium 3 Months",
                    ImageUrl = "https://twemoji.maxcdn.com/v/latest/svg/1f451.svg",
                    PriceCoins = 1400,
                    IsFree = false,
                    DurationMonths = 3,
                    IsActive = true
                },
                new()
                {
                    Id = Guid.Parse("7AB3D2C8-6D4A-46B5-9D0F-5C47D570D5C7"),
                    Type = MarketItemType.PremiumSubscription,
                    Name = "Premium 6 Months",
                    ImageUrl = "https://twemoji.maxcdn.com/v/latest/svg/1f48e.svg",
                    PriceCoins = 2600,
                    IsFree = false,
                    DurationMonths = 6,
                    IsActive = true
                },
                new()
                {
                    Id = Guid.Parse("6E7711AA-80D9-4A2D-9D95-9B2D4C4E8A6B"),
                    Type = MarketItemType.PremiumSubscription,
                    Name = "Premium 12 Months",
                    ImageUrl = "https://twemoji.maxcdn.com/v/latest/svg/1f3c5.svg",
                    PriceCoins = 4800,
                    IsFree = false,
                    DurationMonths = 12,
                    IsActive = true
                }
            ]);
        }

        if (itemsToSeed.Count == 0)
        {
            return;
        }

        await dbContext.MarketItems.AddRangeAsync(itemsToSeed).ConfigureAwait(false);
        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
}
