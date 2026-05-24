using System.ComponentModel.DataAnnotations;
using Quizanchos.WebApi.Validation;

namespace Quizanchos.WebApi.Dto;

public record ConfirmPasswordResetDto(
    [Required, EmailAddress, StringLength(254)] string Email,
    [Required, StringLength(64, MinimumLength = 1)] string Code,
    [Required,
     StringLength(PasswordPolicy.MaxLength, MinimumLength = PasswordPolicy.MinLength,
        ErrorMessage = "New password must be between {2} and {1} characters long"),
     RegularExpression(PasswordPolicy.ComplexityRegex,
        ErrorMessage = PasswordPolicy.Description)]
    string NewPassword);
