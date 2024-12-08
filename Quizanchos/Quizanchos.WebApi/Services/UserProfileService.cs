using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Identity;
using Quizanchos.Common.Util;
using Quizanchos.Domain.Entities;
using Quizanchos.WebApi.Dto;
using System.Security.Claims;

namespace Quizanchos.WebApi.Services;

public class UserProfileService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly UserRetrieverService _userRetrieverService;
    private readonly ICloudinary _cloudinary;

    public UserProfileService(UserManager<ApplicationUser> userManager, UserRetrieverService userRetrieverService, ICloudinary cloudinary)
    {
        _userManager = userManager;
        _userRetrieverService = userRetrieverService;
        _cloudinary = cloudinary;
    }

    public async Task<FullApplicationUserDto> GetUserInfo(ClaimsPrincipal claimsPrincipal)
    {
        ApplicationUser user = await _userRetrieverService.GetUserByClaims(claimsPrincipal).ConfigureAwait(false);
        return new FullApplicationUserDto(user.Email, user.UserName, user.AvatarUrl, user.Score);
    }

    public async Task UpdateNickname(ClaimsPrincipal claimsPrincipal, string nickNameToUpdate)
    {
        ApplicationUser user = await _userRetrieverService.GetUserByClaims(claimsPrincipal).ConfigureAwait(false);
        if(user.UserName == nickNameToUpdate)
        {
            return;
        }

        ApplicationUser? userWithNickname = await _userManager.FindByNameAsync(nickNameToUpdate);
        if(userWithNickname is not null)
        {
            throw HandledExceptionFactory.Create("The user with this nickname exists");
        }

        user.UserName = nickNameToUpdate;
        await _userManager.UpdateAsync(user).ConfigureAwait(false);
    }

    public async Task UpdateAvatarUrl(ClaimsPrincipal claimsPrincipal, string avatarUrl)
    {
        ApplicationUser user = await _userRetrieverService.GetUserByClaims(claimsPrincipal).ConfigureAwait(false);
        user.AvatarUrl = avatarUrl;
        await _userManager.UpdateAsync(user).ConfigureAwait(false);
    }

    public async Task UpdateAvatar(ClaimsPrincipal claimsPrincipal, IFormFile formFile)
    {
        ApplicationUser user = await _userRetrieverService.GetUserByClaims(claimsPrincipal).ConfigureAwait(false);

        if (formFile is null || formFile.Length == 0)
            throw HandledExceptionFactory.Create("No file uploaded.");

        using Stream stream = formFile.OpenReadStream();
        ImageUploadParams uploadParams = new ImageUploadParams
        {
            File = new FileDescription(formFile.FileName, stream)
        };

        ImageUploadResult uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult?.StatusCode != System.Net.HttpStatusCode.OK)
            throw HandledExceptionFactory.Create("Error uploading image.");

        user.AvatarUrl = uploadResult.SecureUrl.AbsoluteUri;
        await _userManager.UpdateAsync(user).ConfigureAwait(false);
    }
}
