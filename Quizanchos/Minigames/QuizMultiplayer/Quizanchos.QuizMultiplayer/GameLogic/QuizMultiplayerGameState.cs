using Quizanchos.Common.Enums;
using Quizanchos.Core;

namespace Quizanchos.QuizMultiplayer.GameLogic;

public class QuizMultiplayerGameState : IGameState
{
    public MinigameType MinigameType => MinigameType.QuizMultiplayer;

    public Guid GameId { get; set; }
    public IReadOnlyList<string> Players { get; set; } = Array.Empty<string>();
    public bool IsFinished { get; set; }
    public string? Winner { get; set; }

    // Quiz configuration (same as Quiz)
    public int CurrentCardIndex { get; set; } = -1;
    public int TotalCards { get; set; }
    public Guid QuizCategoryId { get; set; }
    public GameLevel GameLevel { get; set; }
    public int SecondsPerCard { get; set; }
    public int OptionCount { get; set; }
    public DateTime CreationTime { get; set; }
    public bool IsTerminatedByTime { get; set; }

    // Cards (same structure as Quiz)
    public List<QuizMultiplayerCard> Cards { get; set; } = new();

    // Team data
    public List<TeamData> Teams { get; set; } = new();
    public Dictionary<int, int> TeamScores { get; set; } = new();

    public class QuizMultiplayerCard
    {
        public Guid Id { get; set; }
        public int CardIndex { get; set; }
        public int CorrectOption { get; set; }
        public Dictionary<string, int?> PlayerAnswers { get; set; } = new();
        public Guid[] EntityIds { get; set; } = Array.Empty<Guid>();
        public DateTime CreationTime { get; set; }

        public string[] EntityNames { get; set; } = Array.Empty<string>();
        public object[] OptionValues { get; set; } = Array.Empty<object>();

        /// <summary>
        /// After all players answer: teamIndex -> chosen option (majority vote).
        /// </summary>
        public Dictionary<int, int> TeamVotes { get; set; } = new();
    }

    public class TeamData
    {
        public int TeamIndex { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<string> PlayerIds { get; set; } = new();
    }
}
