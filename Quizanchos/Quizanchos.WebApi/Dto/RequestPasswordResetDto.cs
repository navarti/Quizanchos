using System.ComponentModel.DataAnnotations;

namespace Quizanchos.WebApi.Dto;

public record RequestPasswordResetDto(
    [Required, EmailAddress] string Email);
