using System.ComponentModel.DataAnnotations;

namespace Quizanchos.WebApi.Dto;

public record ContactMessageDto(
    [Required, StringLength(100, MinimumLength = 1)] string Name,
    [Required, EmailAddress, StringLength(256)] string Email,
    [Required, StringLength(200, MinimumLength = 1)] string Subject,
    [Required, StringLength(5000, MinimumLength = 10)] string Message);
