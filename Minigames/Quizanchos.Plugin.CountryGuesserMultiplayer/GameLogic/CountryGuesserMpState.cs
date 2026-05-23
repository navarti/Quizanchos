using Quizanchos.Core;

namespace Quizanchos.Plugin.CountryGuesserMultiplayer.GameLogic;

public sealed class CountryGuesserMpState : IGameState
{
    public int MinigameType => CountryGuesserMpConstants.MinigameTypeId;

    public Guid GameId { get; set; }
    public IReadOnlyList<string> Players { get; set; } = Array.Empty<string>();
    public bool IsFinished { get; set; }
    public string? Winner { get; set; }

    public int CurrentCardIndex { get; set; } = -1;
    public int TotalCards { get; set; } = 5;
    public int SecondsPerCard { get; set; } = 20;

    /// <summary>
    /// Maximum great-circle distance (km) between a click and the country
    /// centroid that still counts as a correct answer.
    /// </summary>
    public double MaxDistanceKm { get; set; } = 600;

    public List<MpCard> Cards { get; set; } = new();
    public Dictionary<string, int> Scores { get; set; } = new();
    public Dictionary<string, string> PlayerNicknames { get; set; } = new();

    public sealed class MpCard
    {
        public int CardIndex { get; set; }
        public string TargetCode { get; set; } = string.Empty;
        public string TargetName { get; set; } = string.Empty;
        public double TargetLat { get; set; }
        public double TargetLon { get; set; }
        public Dictionary<string, ClickAnswer> PlayerAnswers { get; set; } = new();
        public DateTime CreationTime { get; set; }
    }

    public sealed class ClickAnswer
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
        public double DistanceKm { get; set; }
        public bool Correct { get; set; }
    }
}
