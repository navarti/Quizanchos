using Quizanchos.Common.Enums;
namespace Quizanchos.ViewModels
{
    public class SingleGameSessionViewModel
    {
        public Guid Id { get; set; } 
        public string UserId { get; set; } 
        public DateTime CreationTime { get; set; } 
        public int CurrentCardIndex { get; set; } 
        public int Score { get; set; } 
        public bool IsFinished { get; set; } 
        public bool IsTerminatedByTime { get; set; } 
        public int CardsCount { get; set; } 
        public int SecondsPerCard { get; set; } 
        public Guid QuizCategoryId { get; set; } 
        public string QuizCategoryName { get; set; } 
        public string GameLevel { get; set; } 
    }

}