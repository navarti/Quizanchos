using System.Collections.Immutable;
using Quizanchos.Core;
using Quizanchos.Plugin.CountryGuesser.Data;

namespace Quizanchos.Plugin.CountryGuesser.GameLogic;

public sealed class CountryGuesserLogic : IGameLogic<CountryGuesserState, CountryGuesserMove>
{
    private const double EarthRadiusKm = 6371.0088;

    private readonly CountryRepository _repository;
    private readonly int _totalCards;
    private readonly int _secondsPerCard;
    private readonly double _maxDistanceKm;
    private readonly int _seed;

    public CountryGuesserLogic(
        CountryRepository repository,
        int totalCards = 5,
        int secondsPerCard = 20,
        double maxDistanceKm = 600,
        int seed = 0)
    {
        _repository = repository;
        _totalCards = Math.Max(1, totalCards);
        _secondsPerCard = Math.Max(5, secondsPerCard);
        _maxDistanceKm = Math.Max(50, maxDistanceKm);
        _seed = seed == 0 ? Random.Shared.Next() : seed;
    }

    public CountryGuesserState CreateInitialState(Guid gameId, ImmutableArray<string> players)
    {
        var rnd = new Random(_seed);
        var state = new CountryGuesserState
        {
            GameId = gameId,
            Players = players.ToList(),
            CurrentCardIndex = 0,
            TotalCards = _totalCards,
            SecondsPerCard = _secondsPerCard,
            MaxDistanceKm = _maxDistanceKm,
        };
        foreach (var p in state.Players)
        {
            state.Scores[p] = 0;
        }

        var pool = _repository.All.ToList();
        if (pool.Count == 0)
        {
            return state;
        }

        Shuffle(pool, rnd);
        var picks = pool.Take(_totalCards).ToList();

        for (int i = 0; i < picks.Count; i++)
        {
            var target = picks[i];
            state.Cards.Add(new CountryGuesserState.CountryCard
            {
                CardIndex = i,
                TargetCode = target.Code,
                TargetName = target.Name,
                TargetLat = target.Lat,
                TargetLon = target.Lon,
                CreationTime = DateTime.UtcNow,
            });
        }

        return state;
    }

    public MoveResult ValidateMove(CountryGuesserState state, CountryGuesserMove move, string playerId)
    {
        if (!state.Players.Contains(playerId))
        {
            return MoveResult.Failure("Player not in game");
        }
        if (state.CurrentCardIndex < 0 || state.CurrentCardIndex >= state.Cards.Count)
        {
            return MoveResult.Failure("No active card");
        }
        var card = state.Cards[state.CurrentCardIndex];
        if (card.PlayerAnswers.ContainsKey(playerId))
        {
            return MoveResult.Failure("Already answered this card");
        }
        if (move.Lat is null || move.Lon is null)
        {
            return MoveResult.Failure("Click coordinates required");
        }
        if (move.Lat is < -90 or > 90 || move.Lon is < -180 or > 180)
        {
            return MoveResult.Failure("Coordinates out of range");
        }
        return MoveResult.Success;
    }

    public void ApplyMove(CountryGuesserState state, CountryGuesserMove move, string playerId)
    {
        var card = state.Cards[state.CurrentCardIndex];
        double lat = move.Lat!.Value;
        double lon = move.Lon!.Value;
        double distance = HaversineKm(lat, lon, card.TargetLat, card.TargetLon);
        bool correct = distance <= state.MaxDistanceKm;

        card.PlayerAnswers[playerId] = new CountryGuesserState.ClickAnswer
        {
            Lat = lat,
            Lon = lon,
            DistanceKm = distance,
            Correct = correct,
        };

        if (correct)
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

    public bool CheckFinished(CountryGuesserState state)
    {
        return state.CurrentCardIndex >= state.TotalCards;
    }

    public string? DetermineWinner(CountryGuesserState state)
    {
        if (state.Scores.Count == 0) return null;
        int max = state.Scores.Values.Max();
        var top = state.Scores.Where(kv => kv.Value == max).ToList();
        return top.Count == 1 ? top[0].Key : null;
    }

    public IEnumerable<string> GetExpectedPlayers(CountryGuesserState state)
    {
        if (state.CurrentCardIndex < 0 || state.CurrentCardIndex >= state.Cards.Count)
        {
            return Array.Empty<string>();
        }
        var card = state.Cards[state.CurrentCardIndex];
        return state.Players.Where(p => !card.PlayerAnswers.ContainsKey(p));
    }

    public bool NeedToFinish(CountryGuesserState state)
    {
        if (state.CurrentCardIndex < 0 || state.CurrentCardIndex >= state.Cards.Count)
        {
            return false;
        }
        var card = state.Cards[state.CurrentCardIndex];
        return DateTime.UtcNow > card.CreationTime.AddSeconds(state.SecondsPerCard);
    }

    public IReadOnlyDictionary<string, int> GetPlayerScores(CountryGuesserState state)
    {
        return new Dictionary<string, int>(state.Scores);
    }

    private static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
    {
        double phi1 = lat1 * Math.PI / 180.0;
        double phi2 = lat2 * Math.PI / 180.0;
        double dPhi = (lat2 - lat1) * Math.PI / 180.0;
        double dLambda = (lon2 - lon1) * Math.PI / 180.0;

        double a = Math.Sin(dPhi / 2) * Math.Sin(dPhi / 2)
                 + Math.Cos(phi1) * Math.Cos(phi2)
                 * Math.Sin(dLambda / 2) * Math.Sin(dLambda / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return EarthRadiusKm * c;
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
