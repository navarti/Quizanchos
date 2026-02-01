namespace Quizanchos.ViewModels;
using Quizanchos.Quiz.Dto;
using Quizanchos.WebApi.Dto;
public class HomeViewModel
{
    public required List<QuizCategoryDto> QuizCategories { get; set; }
    // TODO:GameSession
    public object? ActiveSession { get; set; } 

    public required string QuizName { get; set; }
    public string FormattedCreationTime => "N/A"; // ActiveSession?.CreationTime.ToString("g") ?? "N/A";
    public string Progress => ActiveSession != null
        ? "N/A" //$"{ActiveSession.CurrentCardIndex}/{(int)ActiveSession.CardsCount}"
        : "N/A";
    public int Score => 0; //ActiveSession?.Score ?? 0;

    public List<ApplicationUserInLeaderBoardDto> Users { get; set; } = new List<ApplicationUserInLeaderBoardDto>();

    public required string CurrentUserName { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
}
