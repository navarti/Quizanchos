using System.ComponentModel.DataAnnotations;

namespace Quizanchos.WebApi.Dto;

public record LoginModelDto(
    [Required, EmailAddress, StringLength(254)] string Email,
    [Required] string Password);
