using System.ComponentModel.DataAnnotations;

namespace Quizanchos.WebApi.Dto;

public record ConfirmPasswordResetDto(
    [Required, EmailAddress] string Email,
    [Required, StringLength(64, MinimumLength = 1)] string Code,
    [Required, MinLength(1)] string NewPassword);
