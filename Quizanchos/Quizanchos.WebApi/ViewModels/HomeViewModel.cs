namespace Quizanchos.ViewModels;

using Quizanchos.Core;
using Quizanchos.Quiz.Dto;
using Quizanchos.WebApi.Dto;

public class HomeViewModel
{
    public required List<QuizCategoryDto> QuizCategories { get; set; }
    public IGameState? ActiveSession { get; set; } 
    public string? ActiveSessionUrl { get; set; }

    public required string QuizName { get; set; }
    public List<MinigameCardViewModel> Minigames { get; set; } = new List<MinigameCardViewModel>();

    public List<ApplicationUserInLeaderBoardDto> Users { get; set; } = new List<ApplicationUserInLeaderBoardDto>();

    public required string CurrentUserName { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
}
