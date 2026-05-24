using System.ComponentModel.DataAnnotations;
using Quizanchos.WebApi.Validation;

namespace Quizanchos.WebApi.Dto;

public record ChangePasswordDto(
    [Required] string CurrentPassword,
    [Required,
     StringLength(PasswordPolicy.MaxLength, MinimumLength = PasswordPolicy.MinLength,
        ErrorMessage = "New password must be between {2} and {1} characters long"),
     RegularExpression(PasswordPolicy.ComplexityRegex,
        ErrorMessage = PasswordPolicy.Description)]
    string NewPassword);
