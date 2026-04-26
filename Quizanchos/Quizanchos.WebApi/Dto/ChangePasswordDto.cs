using System.ComponentModel.DataAnnotations;

namespace Quizanchos.WebApi.Dto;

public record ChangePasswordDto(
    [Required] string CurrentPassword,
    [Required, MinLength(1)] string NewPassword);
