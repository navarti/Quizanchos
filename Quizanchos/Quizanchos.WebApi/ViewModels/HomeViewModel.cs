namespace Quizanchos.ViewModels;
using Quizanchos.WebApi.Dto;
public class HomeViewModel
{
    public List<QuizCategoryDto> QuizCategories { get; set; }
    public SingleGameSessionDto? ActiveSession { get; set; } 

    public string QuizName { get; set; }
    public string FormattedCreationTime => ActiveSession?.CreationTime.ToString("g") ?? "N/A";
    public string Progress => ActiveSession != null
        ? $"{ActiveSession.CurrentCardIndex}/{(int)ActiveSession.CardsCount}"
        : "N/A";
    public int Score => ActiveSession?.Score ?? 0;
    
    public List<ApplicationUserInLeaderBoardDto> Users { get; set; } = new List<ApplicationUserInLeaderBoardDto>();
    
    public string CurrentUserName { get; set; }
}
