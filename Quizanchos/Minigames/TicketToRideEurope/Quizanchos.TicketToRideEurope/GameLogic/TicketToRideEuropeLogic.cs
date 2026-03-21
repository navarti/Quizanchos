using System.Collections.Immutable;
using Quizanchos.Core;

namespace Quizanchos.TicketToRideEurope.GameLogic;

public class TicketToRideEuropeLogic : IGameLogic<TicketToRideEuropeState, TicketToRideEuropeMove>
{
    private readonly Random _random = new();

    public TicketToRideEuropeState CreateInitialState(Guid gameId, ImmutableArray<string> players)
    {
        var state = new TicketToRideEuropeState
        {
            GameId = gameId,
            Players = players.ToList(),
            IsFinished = false,
            Winner = null,
            CreationTime = DateTime.UtcNow,
            CurrentPlayerIndex = 0,
            CurrentPlayerId = players[0],
            RemainingFinalTurns = 0,
            LastRoundTriggered = false,
            TurnNumber = 1,
            Routes = BuildRoutes(),
            Deck = BuildTrainDeck(),
            DestinationDeck = BuildDestinationTickets()
        };

        Shuffle(state.Deck);
        Shuffle(state.DestinationDeck);

        foreach (var playerId in state.Players)
        {
            var playerState = new TicketToRideEuropeState.PlayerState
            {
                PlayerId = playerId
            };

            for (int i = 0; i < 4; i++)
            {
                var card = DrawFromDeck(state);
                if (card.HasValue)
                {
                    playerState.Hand.Add(card.Value);
                }
            }

            var initialTickets = DrawDestinationTickets(state, 3);
            playerState.DestinationTickets = initialTickets.Take(2).ToList();

            state.PlayerStates[playerId] = playerState;
        }

        RefillFaceUpCards(state);
        state.LastActionSummary = "Game started.";

        return state;
    }

    public MoveResult ValidateMove(TicketToRideEuropeState state, TicketToRideEuropeMove move, string playerId)
    {
        if (!state.PlayerStates.ContainsKey(playerId))
        {
            return MoveResult.Failure("Player is not part of this game");
        }

        if (state.CurrentPlayerId != playerId)
        {
            return MoveResult.Failure("It is not your turn");
        }

        if (state.AwaitingTicketSelection && move.Action != TicketToRideEuropeAction.KeepDestinationTickets)
        {
            return MoveResult.Failure("You must resolve destination ticket selection first");
        }

        var playerState = state.PlayerStates[playerId];

        return move.Action switch
        {
            TicketToRideEuropeAction.DrawCards => ValidateDrawCards(state, move),
            TicketToRideEuropeAction.ClaimRoute => ValidateClaimRoute(state, playerState, move),
            TicketToRideEuropeAction.DrawDestinationTickets => ValidateDrawDestinationTickets(state),
            TicketToRideEuropeAction.KeepDestinationTickets => ValidateKeepDestinationTickets(state, move),
            TicketToRideEuropeAction.PlaceStation => ValidatePlaceStation(state, playerState, move),
            _ => MoveResult.Failure("Unknown action")
        };
    }

    public void ApplyMove(TicketToRideEuropeState state, TicketToRideEuropeMove move, string playerId)
    {
        var playerState = state.PlayerStates[playerId];

        switch (move.Action)
        {
            case TicketToRideEuropeAction.DrawCards:
                ApplyDrawCards(state, playerState, move);
                AdvanceTurn(state);
                break;

            case TicketToRideEuropeAction.ClaimRoute:
                ApplyClaimRoute(state, playerState, move);
                AdvanceTurn(state);
                break;

            case TicketToRideEuropeAction.DrawDestinationTickets:
                state.PendingDestinationChoices = DrawDestinationTickets(state, 3);
                state.AwaitingTicketSelection = true;
                state.LastActionSummary = "Picked 3 destination tickets. Keep at least 1.";
                break;

            case TicketToRideEuropeAction.KeepDestinationTickets:
                ApplyKeepDestinationTickets(state, playerState, move);
                AdvanceTurn(state);
                break;

            case TicketToRideEuropeAction.PlaceStation:
                ApplyPlaceStation(playerState, move);
                AdvanceTurn(state);
                break;
        }
    }

    public bool CheckFinished(TicketToRideEuropeState state)
    {
        if (state.LastRoundTriggered && state.RemainingFinalTurns <= 0)
        {
            FinalizeScoring(state);
            return true;
        }

        return false;
    }

    public string? DetermineWinner(TicketToRideEuropeState state)
    {
        if (state.PlayerStates.Count == 0)
            return null;

        var best = state.PlayerStates.Values
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.TrainsRemaining)
            .First();

        return best.PlayerId;
    }

    public IEnumerable<string> GetExpectedPlayers(TicketToRideEuropeState state)
    {
        if (state.IsFinished)
            return Enumerable.Empty<string>();

        return [state.CurrentPlayerId];
    }

    public bool NeedToFinish(TicketToRideEuropeState state)
    {
        return state.LastRoundTriggered && state.RemainingFinalTurns <= 0;
    }

    public IReadOnlyDictionary<string, int> GetPlayerScores(TicketToRideEuropeState state)
    {
        return state.PlayerStates.ToDictionary(x => x.Key, x => x.Value.Score);
    }

    private MoveResult ValidateDrawCards(TicketToRideEuropeState state, TicketToRideEuropeMove move)
    {
        var sources = move.DrawSources ?? new List<string>();
        if (sources.Count is < 1 or > 2)
        {
            return MoveResult.Failure("You must draw 1 or 2 cards");
        }

        if (sources.Count == 2 && sources.Any(x => x.StartsWith("faceup:", StringComparison.OrdinalIgnoreCase)))
        {
            foreach (var source in sources.Where(x => x.StartsWith("faceup:", StringComparison.OrdinalIgnoreCase)))
            {
                if (!TryReadFaceUpIndex(source, out var idx) || idx < 0 || idx >= state.FaceUpCards.Count)
                {
                    return MoveResult.Failure("Invalid face-up card index");
                }
            }

            var first = sources[0];
            if (TryReadFaceUpIndex(first, out var firstIdx) && state.FaceUpCards[firstIdx] == TrainCardColor.Locomotive)
            {
                return MoveResult.Failure("If you take a face-up locomotive, you can only draw one card");
            }
        }

        return MoveResult.Success;
    }

    private MoveResult ValidateClaimRoute(
        TicketToRideEuropeState state,
        TicketToRideEuropeState.PlayerState playerState,
        TicketToRideEuropeMove move)
    {
        if (string.IsNullOrWhiteSpace(move.RouteId))
        {
            return MoveResult.Failure("RouteId is required");
        }

        var route = state.Routes.FirstOrDefault(x => x.Id == move.RouteId);
        if (route == null)
        {
            return MoveResult.Failure("Route not found");
        }

        if (!string.IsNullOrEmpty(route.ClaimedBy))
        {
            return MoveResult.Failure("Route already claimed");
        }

        if (playerState.TrainsRemaining < route.Length)
        {
            return MoveResult.Failure("Not enough train pieces");
        }

        var color = ParseColor(move.Color);
        if (route.Color.HasValue && route.Color != color)
        {
            return MoveResult.Failure("This route requires a specific color");
        }

        if (!CanPayRoute(playerState.Hand, route, color))
        {
            return MoveResult.Failure("Not enough cards to claim this route");
        }

        return MoveResult.Success;
    }

    private MoveResult ValidateDrawDestinationTickets(TicketToRideEuropeState state)
    {
        if (state.DestinationDeck.Count == 0)
        {
            return MoveResult.Failure("No destination tickets left");
        }

        return MoveResult.Success;
    }

    private MoveResult ValidateKeepDestinationTickets(TicketToRideEuropeState state, TicketToRideEuropeMove move)
    {
        if (!state.AwaitingTicketSelection)
        {
            return MoveResult.Failure("No destination ticket selection is pending");
        }

        var keep = move.KeepTicketIndexes ?? new List<int>();
        if (keep.Count < 1)
        {
            return MoveResult.Failure("You must keep at least one destination ticket");
        }

        if (keep.Any(i => i < 0 || i >= state.PendingDestinationChoices.Count))
        {
            return MoveResult.Failure("Invalid destination ticket index");
        }

        return MoveResult.Success;
    }

    private MoveResult ValidatePlaceStation(
        TicketToRideEuropeState state,
        TicketToRideEuropeState.PlayerState playerState,
        TicketToRideEuropeMove move)
    {
        if (playerState.StationsRemaining <= 0)
        {
            return MoveResult.Failure("No stations remaining");
        }

        if (string.IsNullOrWhiteSpace(move.StationCity))
        {
            return MoveResult.Failure("Station city is required");
        }

        if (playerState.StationCities.Contains(move.StationCity, StringComparer.OrdinalIgnoreCase))
        {
            return MoveResult.Failure("You already placed a station in this city");
        }

        int cost = GetStationCost(playerState);
        var color = ParseColor(move.Color);
        if (!CanPayCards(playerState.Hand, color, cost, 0))
        {
            return MoveResult.Failure("Not enough cards to place station");
        }

        return MoveResult.Success;
    }

    private void ApplyDrawCards(TicketToRideEuropeState state, TicketToRideEuropeState.PlayerState playerState, TicketToRideEuropeMove move)
    {
        var sources = move.DrawSources ?? new List<string>();

        for (int i = 0; i < sources.Count; i++)
        {
            var source = sources[i];
            if (TryReadFaceUpIndex(source, out var index) && index >= 0 && index < state.FaceUpCards.Count)
            {
                var card = state.FaceUpCards[index];
                playerState.Hand.Add(card);
                state.FaceUpCards.RemoveAt(index);

                if (i == 0 && card == TrainCardColor.Locomotive)
                {
                    break;
                }
            }
            else
            {
                var card = DrawFromDeck(state);
                if (card.HasValue)
                {
                    playerState.Hand.Add(card.Value);
                }
            }

            RefillFaceUpCards(state);
        }

        state.LastActionSummary = "Drew train cards.";
    }

    private void ApplyClaimRoute(
        TicketToRideEuropeState state,
        TicketToRideEuropeState.PlayerState playerState,
        TicketToRideEuropeMove move)
    {
        var route = state.Routes.First(x => x.Id == move.RouteId);
        var chosenColor = ParseColor(move.Color);

        var handSnapshot = playerState.Hand.ToList();
        bool paid = TryPayRouteCards(state, playerState, route, chosenColor);
        if (!paid)
        {
            playerState.Hand = handSnapshot;
            state.LastActionSummary = $"Failed to claim tunnel {route.CityA} - {route.CityB}: extra cards required.";
            return;
        }

        route.ClaimedBy = playerState.PlayerId;
        playerState.ClaimedRouteIds.Add(route.Id);
        playerState.TrainsRemaining -= route.Length;
        playerState.Score += GetRoutePoints(route.Length);

        state.LastActionSummary = $"Claimed route {route.CityA} - {route.CityB}.";
    }

    private void ApplyKeepDestinationTickets(
        TicketToRideEuropeState state,
        TicketToRideEuropeState.PlayerState playerState,
        TicketToRideEuropeMove move)
    {
        var keep = (move.KeepTicketIndexes ?? new List<int>())
            .Distinct()
            .Select(i => state.PendingDestinationChoices[i])
            .ToList();

        playerState.DestinationTickets.AddRange(keep);
        state.PendingDestinationChoices.Clear();
        state.AwaitingTicketSelection = false;
        state.LastActionSummary = "Destination tickets updated.";
    }

    private void ApplyPlaceStation(TicketToRideEuropeState.PlayerState playerState, TicketToRideEuropeMove move)
    {
        int cost = GetStationCost(playerState);
        var color = ParseColor(move.Color);
        PayCards(playerState.Hand, color, cost, 0);

        playerState.StationsRemaining -= 1;
        playerState.StationCities.Add(move.StationCity!);
    }

    private void AdvanceTurn(TicketToRideEuropeState state)
    {
        var currentPlayer = state.PlayerStates[state.CurrentPlayerId];
        if (!state.LastRoundTriggered && currentPlayer.TrainsRemaining <= 2)
        {
            state.LastRoundTriggered = true;
            state.RemainingFinalTurns = state.Players.Count;
            state.LastActionSummary += " Final round triggered!";
        }

        state.CurrentPlayerIndex = (state.CurrentPlayerIndex + 1) % state.Players.Count;
        state.CurrentPlayerId = state.Players[state.CurrentPlayerIndex];
        state.TurnNumber++;

        if (state.LastRoundTriggered)
        {
            state.RemainingFinalTurns -= 1;
        }
    }

    private void FinalizeScoring(TicketToRideEuropeState state)
    {
        var longestPathOwner = GetLongestPathOwner(state);

        foreach (var player in state.PlayerStates.Values)
        {
            foreach (var ticket in player.DestinationTickets)
            {
                if (IsTicketCompleted(state, player, ticket))
                {
                    player.Score += ticket.Points;
                }
                else
                {
                    player.Score -= ticket.Points;
                }
            }

            player.Score += player.StationsRemaining * 4;
        }

        if (!string.IsNullOrWhiteSpace(longestPathOwner) && state.PlayerStates.TryGetValue(longestPathOwner, out var longest))
        {
            longest.Score += 10;
        }
    }

    private bool IsTicketCompleted(
        TicketToRideEuropeState state,
        TicketToRideEuropeState.PlayerState player,
        TicketToRideEuropeState.DestinationTicket ticket)
    {
        var adjacency = BuildAdjacencyForPlayer(state, player);
        return HasPath(adjacency, ticket.CityA, ticket.CityB);
    }

    private Dictionary<string, List<string>> BuildAdjacencyForPlayer(
        TicketToRideEuropeState state,
        TicketToRideEuropeState.PlayerState player)
    {
        var map = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        void AddEdge(string a, string b)
        {
            if (!map.TryGetValue(a, out var first))
            {
                first = new List<string>();
                map[a] = first;
            }
            if (!map.TryGetValue(b, out var second))
            {
                second = new List<string>();
                map[b] = second;
            }

            first.Add(b);
            second.Add(a);
        }

        foreach (var route in state.Routes.Where(x => x.ClaimedBy == player.PlayerId))
        {
            AddEdge(route.CityA, route.CityB);
        }

        foreach (var city in player.StationCities)
        {
            var borrowed = state.Routes
                .Where(x => x.ClaimedBy != null && x.ClaimedBy != player.PlayerId)
                .Where(x => x.CityA.Equals(city, StringComparison.OrdinalIgnoreCase)
                            || x.CityB.Equals(city, StringComparison.OrdinalIgnoreCase));

            foreach (var route in borrowed)
            {
                AddEdge(route.CityA, route.CityB);
            }
        }

        return map;
    }

    private static bool HasPath(Dictionary<string, List<string>> adjacency, string from, string to)
    {
        if (from.Equals(to, StringComparison.OrdinalIgnoreCase))
            return true;

        if (!adjacency.ContainsKey(from) || !adjacency.ContainsKey(to))
            return false;

        var queue = new Queue<string>();
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        queue.Enqueue(from);
        visited.Add(from);

        while (queue.Count > 0)
        {
            var city = queue.Dequeue();
            if (!adjacency.TryGetValue(city, out var neighbors))
                continue;

            foreach (var next in neighbors)
            {
                if (!visited.Add(next))
                    continue;

                if (next.Equals(to, StringComparison.OrdinalIgnoreCase))
                    return true;

                queue.Enqueue(next);
            }
        }

        return false;
    }

    private string? GetLongestPathOwner(TicketToRideEuropeState state)
    {
        string? bestPlayer = null;
        int bestLength = -1;

        foreach (var player in state.PlayerStates.Values)
        {
            int length = player.ClaimedRouteIds
                .Select(id => state.Routes.FirstOrDefault(r => r.Id == id)?.Length ?? 0)
                .Sum();

            if (length > bestLength)
            {
                bestLength = length;
                bestPlayer = player.PlayerId;
            }
        }

        return bestPlayer;
    }

    private static List<TicketToRideEuropeState.RouteState> BuildRoutes()
    {
        return new List<TicketToRideEuropeState.RouteState>
        {
            new() { Id = "paris-brussels", CityA = "Paris", CityB = "Brussels", Length = 2, Color = TrainCardColor.Red },
            new() { Id = "paris-frankfurt", CityA = "Paris", CityB = "Frankfurt", Length = 3, Color = null },
            new() { Id = "paris-zurich", CityA = "Paris", CityB = "Zurich", Length = 3, Color = TrainCardColor.Blue, IsTunnel = true },
            new() { Id = "london-brussels", CityA = "London", CityB = "Brussels", Length = 2, Color = null },
            new() { Id = "brussels-amsterdam", CityA = "Brussels", CityB = "Amsterdam", Length = 1, Color = TrainCardColor.Yellow },
            new() { Id = "amsterdam-essen", CityA = "Amsterdam", CityB = "Essen", Length = 3, Color = TrainCardColor.Black },
            new() { Id = "frankfurt-berlin", CityA = "Frankfurt", CityB = "Berlin", Length = 3, Color = TrainCardColor.Green },
            new() { Id = "berlin-warsaw", CityA = "Berlin", CityB = "Warsaw", Length = 4, Color = null },
            new() { Id = "warsaw-vienna", CityA = "Warsaw", CityB = "Vienna", Length = 4, Color = TrainCardColor.White },
            new() { Id = "vienna-budapest", CityA = "Vienna", CityB = "Budapest", Length = 1, Color = TrainCardColor.Orange },
            new() { Id = "vienna-venice", CityA = "Vienna", CityB = "Venice", Length = 4, Color = TrainCardColor.Purple, IsTunnel = true },
            new() { Id = "venice-rome", CityA = "Venice", CityB = "Rome", Length = 2, Color = TrainCardColor.Green },
            new() { Id = "zurich-venice", CityA = "Zurich", CityB = "Venice", Length = 2, Color = null, IsTunnel = true },
            new() { Id = "madrid-paris", CityA = "Madrid", CityB = "Paris", Length = 3, Color = TrainCardColor.Orange },
            new() { Id = "madrid-barcelona", CityA = "Madrid", CityB = "Barcelona", Length = 2, Color = null },
            new() { Id = "barcelona-marseille", CityA = "Barcelona", CityB = "Marseille", Length = 4, Color = TrainCardColor.Purple },
            new() { Id = "marseille-zurich", CityA = "Marseille", CityB = "Zurich", Length = 2, Color = TrainCardColor.Yellow },
            new() { Id = "athens-sofia", CityA = "Athens", CityB = "Sofia", Length = 3, Color = TrainCardColor.Blue },
            new() { Id = "sofia-budapest", CityA = "Sofia", CityB = "Budapest", Length = 4, Color = null, IsTunnel = true },
            new() { Id = "rome-athens", CityA = "Rome", CityB = "Athens", Length = 4, Color = TrainCardColor.White, FerryLocomotivesRequired = 1 }
        };
    }

    private static List<TicketToRideEuropeState.DestinationTicket> BuildDestinationTickets()
    {
        return new List<TicketToRideEuropeState.DestinationTicket>
        {
            new() { CityA = "London", CityB = "Berlin", Points = 8 },
            new() { CityA = "Paris", CityB = "Vienna", Points = 8 },
            new() { CityA = "Madrid", CityB = "Rome", Points = 8 },
            new() { CityA = "Amsterdam", CityB = "Budapest", Points = 10 },
            new() { CityA = "Barcelona", CityB = "Warsaw", Points = 11 },
            new() { CityA = "Athens", CityB = "Brussels", Points = 13 },
            new() { CityA = "London", CityB = "Athens", Points = 17 },
            new() { CityA = "Madrid", CityB = "Berlin", Points = 13 },
            new() { CityA = "Paris", CityB = "Athens", Points = 14 },
            new() { CityA = "Rome", CityB = "Warsaw", Points = 10 }
        };
    }

    private static List<TrainCardColor> BuildTrainDeck()
    {
        var colors = Enum.GetValues<TrainCardColor>().Where(c => c != TrainCardColor.Locomotive).ToList();
        var deck = new List<TrainCardColor>();

        foreach (var color in colors)
        {
            deck.AddRange(Enumerable.Repeat(color, 12));
        }

        deck.AddRange(Enumerable.Repeat(TrainCardColor.Locomotive, 14));
        return deck;
    }

    private static int GetRoutePoints(int length)
    {
        return length switch
        {
            <= 1 => 1,
            2 => 2,
            3 => 4,
            4 => 7,
            5 => 10,
            _ => 15
        };
    }

    private static int GetStationCost(TicketToRideEuropeState.PlayerState playerState)
    {
        int used = 3 - playerState.StationsRemaining;
        return used + 1;
    }

    private bool CanPayRoute(List<TrainCardColor> hand, TicketToRideEuropeState.RouteState route, TrainCardColor chosenColor)
    {
        if (route.Color.HasValue)
        {
            chosenColor = route.Color.Value;
        }

        int requiredColorCards = route.Length - route.FerryLocomotivesRequired;
        if (requiredColorCards < 0)
            return false;

        return CanPayCards(hand, chosenColor, requiredColorCards, route.FerryLocomotivesRequired);
    }

    private bool CanPayCards(List<TrainCardColor> hand, TrainCardColor chosenColor, int colorCount, int locomotiveCount)
    {
        int actualLocos = hand.Count(c => c == TrainCardColor.Locomotive);
        int actualColor = hand.Count(c => c == chosenColor);

        if (actualLocos < locomotiveCount)
            return false;

        int needColorAfterUsingLocosForMandatory = colorCount;
        int spareLocos = actualLocos - locomotiveCount;

        if (actualColor + spareLocos < needColorAfterUsingLocosForMandatory)
            return false;

        return true;
    }

    private bool TryPayRouteCards(
        TicketToRideEuropeState state,
        TicketToRideEuropeState.PlayerState playerState,
        TicketToRideEuropeState.RouteState route,
        TrainCardColor chosenColor)
    {
        if (route.Color.HasValue)
        {
            chosenColor = route.Color.Value;
        }

        int colorCount = route.Length - route.FerryLocomotivesRequired;
        PayCards(playerState.Hand, chosenColor, colorCount, route.FerryLocomotivesRequired);

        if (route.IsTunnel)
        {
            int extraCost = 0;
            for (int i = 0; i < 3; i++)
            {
                var card = DrawFromDeck(state);
                if (!card.HasValue)
                    continue;

                bool isMatch = card.Value == chosenColor || card.Value == TrainCardColor.Locomotive;
                if (isMatch)
                {
                    extraCost++;
                }
            }

            if (extraCost > 0)
            {
                if (!CanPayCards(playerState.Hand, chosenColor, extraCost, 0))
                {
                    return false;
                }

                PayCards(playerState.Hand, chosenColor, extraCost, 0);
                state.LastActionSummary += $" Tunnel extra cost paid: {extraCost}.";
            }
        }

        return true;
    }

    private static void PayCards(List<TrainCardColor> hand, TrainCardColor chosenColor, int colorCount, int locomotiveCount)
    {
        RemoveCards(hand, TrainCardColor.Locomotive, locomotiveCount);

        int availableColor = hand.Count(c => c == chosenColor);
        int useColor = Math.Min(availableColor, colorCount);
        if (useColor > 0)
        {
            RemoveCards(hand, chosenColor, useColor);
        }

        int remaining = colorCount - useColor;
        if (remaining > 0)
        {
            RemoveCards(hand, TrainCardColor.Locomotive, remaining);
        }
    }

    private static void RemoveCards(List<TrainCardColor> hand, TrainCardColor color, int count)
    {
        for (int i = 0; i < count; i++)
        {
            int idx = hand.FindIndex(c => c == color);
            if (idx >= 0)
            {
                hand.RemoveAt(idx);
            }
        }
    }

    private TrainCardColor? DrawFromDeck(TicketToRideEuropeState state)
    {
        if (state.Deck.Count == 0)
            return null;

        var card = state.Deck[0];
        state.Deck.RemoveAt(0);
        return card;
    }

    private void RefillFaceUpCards(TicketToRideEuropeState state)
    {
        while (state.FaceUpCards.Count < 5 && state.Deck.Count > 0)
        {
            var card = DrawFromDeck(state);
            if (card.HasValue)
            {
                state.FaceUpCards.Add(card.Value);
            }
        }
    }

    private List<TicketToRideEuropeState.DestinationTicket> DrawDestinationTickets(TicketToRideEuropeState state, int count)
    {
        var result = state.DestinationDeck.Take(count).ToList();
        state.DestinationDeck = state.DestinationDeck.Skip(result.Count).ToList();
        return result;
    }

    private static bool TryReadFaceUpIndex(string source, out int index)
    {
        index = -1;
        if (!source.StartsWith("faceup:", StringComparison.OrdinalIgnoreCase))
            return false;

        return int.TryParse(source.Split(':').LastOrDefault(), out index);
    }

    private TrainCardColor ParseColor(string? color)
    {
        if (Enum.TryParse<TrainCardColor>(color, true, out var parsed))
        {
            return parsed;
        }

        return TrainCardColor.Red;
    }

    private void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = _random.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
