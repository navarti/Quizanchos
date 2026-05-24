using System.ComponentModel.DataAnnotations;
using Quizanchos.WebApi.Validation;

namespace Quizanchos.WebApi.Dto;

public record RegisterModelDto(
    [Required, EmailAddress, StringLength(254)] string Email,
    [Required,
     StringLength(PasswordPolicy.MaxLength, MinimumLength = PasswordPolicy.MinLength,
        ErrorMessage = "Password must be between {2} and {1} characters long"),
     RegularExpression(PasswordPolicy.ComplexityRegex,
        ErrorMessage = PasswordPolicy.Description)]
    string Password);
