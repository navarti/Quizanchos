namespace Quizanchos.WebApi.Constants;

public static class Roles
{
    public static readonly List<string> All = new List<string> { User, Admin };
    
    public const string User = "User";
    public const string Admin = "Admin";
}
