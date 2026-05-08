using System.Collections.Immutable;
using Quizanchos.Core;
using Quizanchos.Plugin.CountryGuesser.Data;

namespace Quizanchos.Plugin.CountryGuesserMultiplayer.GameLogic;

public sealed class CountryGuesserMpLogic : IGameLogic<CountryGuesserMpState, CountryGuesserMpMove>
{
    private readonly CountryRepository _repository;
    private readonly int _totalCards;
    private readonly int _optionCount;
    private readonly int _secondsPerCard;
    private readonly int _seed;

    public CountryGuesserMpLogic(
        CountryRepository repository,
        int totalCards = 5,
        int optionCount = 4,
        int secondsPerCard = 20,
        int seed = 0)
    {
        _repository = repository;
        _totalCards = Math.Max(1, totalCards);
        _optionCount = Math.Clamp(optionCount, 2, 6);
        _secondsPerCard = Math.Max(5, secondsPerCard);
        _seed = seed == 0 ? Random.Shared.Next() : seed;
    }

    public CountryGuesserMpState CreateInitialState(Guid gameId, ImmutableArray<string> players)
    {
        var rnd = new Random(_seed);
        var state = new CountryGuesserMpState
        {
            GameId = gameId,
            Players = players.ToList(),
            CurrentCardIndex = 0,
            TotalCards = _totalCards,
            OptionCount = _optionCount,
            SecondsPerCard = _secondsPerCard,
        };
        foreach (var p in state.Players) state.Scores[p] = 0;

        var pool = _repository.All.ToList();
        if (pool.Count == 0) return state;

        Shuffle(pool, rnd);
        var picks = pool.Take(_totalCards).ToList();

        for (int i = 0; i < picks.Count; i++)
        {
            var target = picks[i];
            var distractors = pool
                .Where(c => c.Code != target.Code)
                .OrderBy(_ => rnd.Next())
                .Take(_optionCount - 1)
                .ToList();
            var allOpts = distractors.Append(target).OrderBy(_ => rnd.Next()).ToList();
            int correctIdx = allOpts.FindIndex(c => c.Code == target.Code);

            state.Cards.Add(new CountryGuesserMpState.MpCard
            {
                CardIndex = i,
                TargetCode = target.Code,
                TargetName = target.Name,
                TargetLat = target.Lat,
                TargetLon = target.Lon,
                OptionCodes = allOpts.Select(c => c.Code).ToList(),
                OptionNames = allOpts.Select(c => c.Name).ToList(),
                CorrectOption = correctIdx,
                CreationTime = DateTime.UtcNow,
            });
        }

        return state;
    }

    public MoveResult ValidateMove(CountryGuesserMpState state, CountryGuesserMpMove move, string playerId)
    {
        if (!state.Players.Contains(playerId))
            return MoveResult.Failure("Player not in game");
        if (state.CurrentCardIndex < 0 || state.CurrentCardIndex >= state.Cards.Count)
            return MoveResult.Failure("No active card");
        var card = state.Cards[state.CurrentCardIndex];
        if (card.PlayerAnswers.ContainsKey(playerId))
            return MoveResult.Failure("Already answered this round");
        if (move.OptionPicked < 0 || move.OptionPicked >= card.OptionCodes.Count)
            return MoveResult.Failure("Invalid option");
        return MoveResult.Success;
    }

    public void ApplyMove(CountryGuesserMpState state, CountryGuesserMpMove move, string playerId)
    {
        var card = state.Cards[state.CurrentCardIndex];
        card.PlayerAnswers[playerId] = move.OptionPicked;

        if (move.OptionPicked == card.CorrectOption)
        {
            state.Scores[playerId] = state.Scores.GetValueOrDefault(playerId, 0) + 1;
        }

        if (card.PlayerAnswers.Count >= state.Players.Count)
        {
            state.CurrentCardIndex++;
            if (state.CurrentCardIndex < state.Cards.Count)
            {
                state.Cards[state.CurrentCardIndex].CreationTime = DateTime.UtcNow;
            }
        }
    }

    public bool CheckFinished(CountryGuesserMpState state)
    {
        return state.CurrentCardIndex >= state.TotalCards;
    }

    public string? DetermineWinner(CountryGuesserMpState state)
    {
        if (state.Scores.Count == 0) return null;
        int max = state.Scores.Values.Max();
        var top = state.Scores.Where(kv => kv.Value == max).ToList();
        return top.Count == 1 ? top[0].Key : null;
    }

    public IEnumerable<string> GetExpectedPlayers(CountryGuesserMpState state)
    {
        if (state.CurrentCardIndex < 0 || state.CurrentCardIndex >= state.Cards.Count)
            return Array.Empty<string>();
        var card = state.Cards[state.CurrentCardIndex];
        return state.Players.Where(p => !card.PlayerAnswers.ContainsKey(p));
    }

    public bool NeedToFinish(CountryGuesserMpState state)
    {
        if (state.CurrentCardIndex < 0 || state.CurrentCardIndex >= state.Cards.Count)
            return false;
        var card = state.Cards[state.CurrentCardIndex];
        return DateTime.UtcNow > card.CreationTime.AddSeconds(state.SecondsPerCard);
    }

    public IReadOnlyDictionary<string, int> GetPlayerScores(CountryGuesserMpState state)
    {
        // Multiplayer scoring: winner gets 5 pts, draw gives 2 pts to top, others 0.
        var scores = new Dictionary<string, int>();
        if (state.Scores.Count == 0)
            return scores;

        int max = state.Scores.Values.Max();
        var winners = state.Scores.Where(kv => kv.Value == max).Select(kv => kv.Key).ToHashSet();
        bool draw = winners.Count > 1;

        foreach (var p in state.Players)
        {
            if (winners.Contains(p))
                scores[p] = draw ? 2 : 5;
            else
                scores[p] = 0;
        }
        return scores;
    }

    private static void Shuffle<T>(IList<T> list, Random rnd)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rnd.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
