using System.Text.RegularExpressions;
using Quizanchos.Common.Util;

namespace Quizanchos.WebApi.Validation;

public static class PasswordPolicy
{
    public const int MinLength = 8;
    public const int MaxLength = 128;
    public const bool RequireDigit = true;
    public const bool RequireLowercase = true;
    public const bool RequireUppercase = true;
    public const bool RequireNonAlphanumeric = false;
    public const int RequiredUniqueChars = 4;

    public const string ComplexityRegex = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$";

    public const string Description =
        "Password must be 8-128 characters long, contain at least one uppercase letter, one lowercase letter, one digit, and no whitespace characters.";

    private static readonly Regex CompiledComplexityRegex = new(ComplexityRegex, RegexOptions.Compiled);

    public static void Validate(string? password, string fieldName = "Password")
    {
        if (string.IsNullOrEmpty(password))
        {
            throw HandledExceptionFactory.Create($"{fieldName} is required");
        }

        if (password.Length < MinLength)
        {
            throw HandledExceptionFactory.Create($"{fieldName} must be at least {MinLength} characters long");
        }

        if (password.Length > MaxLength)
        {
            throw HandledExceptionFactory.Create($"{fieldName} must be at most {MaxLength} characters long");
        }

        if (ContainsWhitespace(password))
        {
            throw HandledExceptionFactory.Create($"{fieldName} must not contain whitespace characters");
        }

        if (CountUniqueChars(password) < RequiredUniqueChars)
        {
            throw HandledExceptionFactory.Create($"{fieldName} must contain at least {RequiredUniqueChars} different characters");
        }

        if (!CompiledComplexityRegex.IsMatch(password))
        {
            throw HandledExceptionFactory.Create(Description);
        }
    }

    private static bool ContainsWhitespace(string value)
    {
        foreach (char c in value)
        {
            if (char.IsWhiteSpace(c))
            {
                return true;
            }
        }
        return false;
    }

    private static int CountUniqueChars(string value)
    {
        HashSet<char> seen = new();
        foreach (char c in value)
        {
            seen.Add(c);
        }
        return seen.Count;
    }
}
