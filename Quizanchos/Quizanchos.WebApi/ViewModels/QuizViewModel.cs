using System;
using System.Collections.Generic;

namespace Quizanchos.ViewModels
{
    public class QuizViewModel
    {
        public Guid SessionId { get; set; }
        public DateTime CreationTime { get; set; }
        public int SecondsPerCard { get; set; }
        public int OptionCount { get; set; }
        
        public int Score { get; set; } 
        public Guid CategoryId { get; set; }
        public string QuizCategoryName { get; set; }
        public string ImageUrl { get; set; }
        
        public int CurrentCardIndex { get; set; }
        
        public int TotalCards { get; set; }
        
        public QuizOptionViewModel[] Options { get; set; } = Array.Empty<QuizOptionViewModel>();
    }

    public class QuizOptionViewModel
    {
        public Guid Id { get; set; } 
        public string Name { get; set; } 
    }
}