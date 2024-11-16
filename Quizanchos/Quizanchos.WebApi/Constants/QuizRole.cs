namespace Quizanchos.WebApi.Constants;

public static class QuizRole
{
    public static readonly List<string> All = new List<string> { User, Admin, Owner };
    
    public const string User = "User";
    public const string Admin = "Admin";
    public const string Owner = "Owner";
}
