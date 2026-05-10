using Quizanchos.Core;
using System.Collections.Immutable;

namespace Quizanchos.Plugin.TicketToRideEurope.GameLogic;

using State = TicketToRideEuropeState;
using Move = TicketToRideEuropeMove;
using Map = TicketToRideEuropeMap;

public class TicketToRideEuropeLogic : IGameLogic<TicketToRideEuropeState, TicketToRideEuropeMove>
{
    private const int InitialHandSize = 4;
    private const int InitialTicketCount = 4;
    private const int InitialTicketsMustKeep = 2;
    private const int DrawnTicketCount = 3;
    private const int DrawnTicketsMustKeep = 1;
    private const int FaceUpSize = 5;
    private const int LocomotiveReshuffleThreshold = 3;
    private const int InitialTrains = 45;
    private const int MaxStations = 3;
    private const int LongestPathBonus = 10;
    private const int StationBonus = 4;

    public TicketToRideEuropeLogic()
    {
    }

    // ---- IGameStateFactory --------------------------------------------------

    public State CreateInitialState(Guid gameId, ImmutableArray<string> players)
    {
        Random random = new Random(unchecked(gameId.GetHashCode() ^ Environment.TickCount));

        State state = new State
        {
            GameId = gameId,
            Players = players.ToList(),
            IsFinished = false,
            Winner = null,
            Phase = State.PhaseInit,
            PendingAction = null,
            CurrentPlayerIndex = 0,
            LastRoundTriggered = false,
            FinalTurnsRemaining = 0,
            CreationTime = DateTime.UtcNow,
            TurnNumber = 0,
            NextStationCost = 1
        };

        // Build and shuffle train deck
        List<string> deck = Map.BuildTrainCardDeck();
        Shuffle(deck, random);
        state.TrainDeck = deck;

        // Build and shuffle tickets, splitting long vs regular
        List<State.TicketCard> tickets = Map.ValidatedTickets
            .Select(t => new State.TicketCard
            {
                Id = t.Id,
                CityA = t.CityA,
                CityB = t.CityB,
                Points = t.Points,
                IsLong = t.IsLong
            })
            .ToList();

        Shuffle(tickets, random);
        state.TicketDeck = tickets;

        // Set up player states
        for (int i = 0; i < players.Length; i++)
        {
            string playerColor = Map.PlayerColors[i % Map.PlayerColors.Length];
            State.PlayerInfo info = new State.PlayerInfo
            {
                PlayerId = players[i],
                Color = playerColor,
                Order = i,
                TrainsRemaining = InitialTrains,
                StationsBuilt = 0,
                Score = 0,
                Hand = new Dictionary<string, int>(),
                Tickets = new List<State.TicketCard>(),
                PendingTickets = new List<State.TicketCard>(),
                PendingMinKeep = InitialTicketsMustKeep,
                HasPickedInitialTickets = false,
                HasTakenFinalTurn = false
            };

            // Initialize hand colors at zero so JSON keys exist
            foreach (string color in Map.TrainColors) info.Hand[color] = 0;
            info.Hand[Map.ColorLocomotive] = 0;

            // Deal 4 train cards
            for (int j = 0; j < InitialHandSize; j++)
            {
                string card = DrawFromDeck(state, random);
                if (card != "") info.Hand[card]++;
            }

            // Deal 1 long ticket (if available) + 3 regular as PendingTickets
            State.TicketCard? longTicket = state.TicketDeck.FirstOrDefault(t => t.IsLong);
            if (longTicket != null)
            {
                state.TicketDeck.Remove(longTicket);
                info.PendingTickets.Add(longTicket);
            }
            for (int j = 0; j < 3; j++)
            {
                State.TicketCard? regular = state.TicketDeck.FirstOrDefault(t => !t.IsLong);
                if (regular != null)
                {
                    state.TicketDeck.Remove(regular);
                    info.PendingTickets.Add(regular);
                }
            }

            state.PlayerStates.Add(info);
        }

        // Set up 5 face-up cards (with locomotive reshuffle)
        RefillFaceUp(state, random);

        AppendLog(state, "system", "Game started. Choose your destination tickets.");

        return state;
    }

    // ---- IGameValidator -----------------------------------------------------

    public MoveResult ValidateMove(State state, Move move, string playerId)
    {
        State.PlayerInfo? player = state.PlayerStates.FirstOrDefault(p => p.PlayerId == playerId);
        if (player == null)
            return MoveResult.Failure("Player is not part of this game");

        if (state.Phase == State.PhaseEnded)
            return MoveResult.Failure("Game has ended");

        // Resign is allowed at any time, in either phase, irrespective of whose turn it is.
        if (move.Kind == "resign")
        {
            if (player.HasResigned) return MoveResult.Failure("You have already resigned");
            return MoveResult.Success;
        }

        if (player.HasResigned)
            return MoveResult.Failure("You have resigned from this game");

        // Init phase: only keepTickets allowed
        if (state.Phase == State.PhaseInit)
        {
            if (move.Kind != "keepTickets")
                return MoveResult.Failure("During setup you can only choose your starting tickets");
            if (player.HasPickedInitialTickets)
                return MoveResult.Failure("You have already chosen your tickets");
            return ValidateKeepTickets(player, move);
        }

        // Play phase
        if (state.Players[state.CurrentPlayerIndex] != playerId)
            return MoveResult.Failure("It is not your turn");

        if (state.PendingAction == State.PendingTunnelDecision)
        {
            if (state.PendingTunnel == null || state.PendingTunnel.PlayerId != playerId)
                return MoveResult.Failure("No pending tunnel decision");
            if (move.Kind == "tunnelPay") return ValidateTunnelPay(state, player, move);
            if (move.Kind == "tunnelSkip") return MoveResult.Success;
            return MoveResult.Failure("Resolve the pending tunnel first");
        }

        if (state.PendingAction == State.PendingKeepDrawnTickets)
        {
            if (move.Kind != "keepTickets")
                return MoveResult.Failure("Choose which drawn tickets to keep first");
            return ValidateKeepTickets(player, move);
        }

        if (state.PendingAction == State.PendingDrawSecondCard)
        {
            if (move.Kind != "drawDeck" && move.Kind != "drawFace")
                return MoveResult.Failure("Draw your second train card");
            return move.Kind == "drawFace"
                ? ValidateDrawFace(state, move, secondDraw: true)
                : ValidateDrawDeck(state);
        }

        // Free turn — may pick any of the four actions
        return move.Kind switch
        {
            "drawDeck" => ValidateDrawDeck(state),
            "drawFace" => ValidateDrawFace(state, move, secondDraw: false),
            "claimRoute" => ValidateClaimRoute(state, player, move),
            "drawTickets" => ValidateDrawTickets(state),
            "buildStation" => ValidateBuildStation(state, player, move),
            _ => MoveResult.Failure($"Unknown action '{move.Kind}'")
        };
    }

    private static MoveResult ValidateKeepTickets(State.PlayerInfo player, Move move)
    {
        if (move.KeepFlags == null || move.KeepFlags.Length != player.PendingTickets.Count)
            return MoveResult.Failure("KeepFlags length must match the number of pending tickets");
        int keptCount = move.KeepFlags.Count(b => b);
        if (keptCount < player.PendingMinKeep)
            return MoveResult.Failure($"You must keep at least {player.PendingMinKeep} ticket(s)");
        return MoveResult.Success;
    }

    private static MoveResult ValidateDrawDeck(State state)
    {
        if (state.TrainDeck.Count == 0 && state.TrainDiscard.Count == 0)
            return MoveResult.Failure("Train deck is empty");
        return MoveResult.Success;
    }

    private static MoveResult ValidateDrawFace(State state, Move move, bool secondDraw)
    {
        if (move.FaceIndex < 0 || move.FaceIndex >= state.FaceUp.Count)
            return MoveResult.Failure("Invalid face-up card index");
        string? card = state.FaceUp[move.FaceIndex];
        if (card == null)
            return MoveResult.Failure("That face-up slot is empty");
        if (secondDraw && card == Map.ColorLocomotive)
            return MoveResult.Failure("Cannot pick a face-up locomotive as your second card");
        return MoveResult.Success;
    }

    private static MoveResult ValidateDrawTickets(State state)
    {
        if (state.TicketDeck.Count == 0)
            return MoveResult.Failure("No more destination tickets available");
        return MoveResult.Success;
    }

    private MoveResult ValidateClaimRoute(State state, State.PlayerInfo player, Move move)
    {
        if (move.RouteId == null) return MoveResult.Failure("RouteId is required");
        if (!Map.RouteById.TryGetValue(move.RouteId, out Map.RouteInfo? route))
            return MoveResult.Failure("Unknown route");
        if (state.ClaimedRoutes.Any(c => c.RouteId == route.Id))
            return MoveResult.Failure("Route is already claimed");
        if (player.TrainsRemaining < route.Length)
            return MoveResult.Failure("Not enough trains");

        return ValidateRoutePayment(player, route, move.Color, move.Locomotives, isTunnel: false);
    }

    private MoveResult ValidateRoutePayment(State.PlayerInfo player, Map.RouteInfo route,
        string? color, int locomotives, bool isTunnel)
    {
        if (locomotives < 0) return MoveResult.Failure("Locomotives cannot be negative");
        if (locomotives < route.FerryLocomotives)
            return MoveResult.Failure($"Ferry route requires at least {route.FerryLocomotives} locomotive(s)");

        int colorCardsNeeded = route.Length - locomotives;
        if (colorCardsNeeded < 0)
            return MoveResult.Failure("Too many locomotives offered");

        if (colorCardsNeeded > 0)
        {
            if (string.IsNullOrEmpty(color))
                return MoveResult.Failure("Color is required when paying with non-locomotive cards");
            if (color == Map.ColorLocomotive)
                return MoveResult.Failure("Use the locomotives field for locomotive cards");
            if (route.Color != Map.ColorGray && route.Color != color)
                return MoveResult.Failure($"This route requires {route.Color} cards");
        }

        if (locomotives > 0 && !player.Hand.TryGetValue(Map.ColorLocomotive, out int locos) || locomotives > player.Hand.GetValueOrDefault(Map.ColorLocomotive))
            return MoveResult.Failure("Not enough locomotive cards");

        if (colorCardsNeeded > 0 && player.Hand.GetValueOrDefault(color!) < colorCardsNeeded)
            return MoveResult.Failure($"Not enough {color} cards");

        return MoveResult.Success;
    }

    private MoveResult ValidateBuildStation(State state, State.PlayerInfo player, Move move)
    {
        if (player.StationsBuilt >= MaxStations)
            return MoveResult.Failure("You have used all your stations");
        if (string.IsNullOrEmpty(move.CityId) || !Map.CityById.ContainsKey(move.CityId))
            return MoveResult.Failure("Unknown city");
        if (state.Stations.Any(s => s.CityId == move.CityId))
            return MoveResult.Failure("City already has a station");

        int costCards = player.StationsBuilt + 1;
        if (move.Locomotives < 0 || move.Locomotives > costCards)
            return MoveResult.Failure("Invalid locomotive count");

        int colorCardsNeeded = costCards - move.Locomotives;
        if (colorCardsNeeded > 0)
        {
            if (string.IsNullOrEmpty(move.Color))
                return MoveResult.Failure("Color is required");
            if (move.Color == Map.ColorLocomotive)
                return MoveResult.Failure("Use the locomotives field for locomotive cards");
            if (player.Hand.GetValueOrDefault(move.Color) < colorCardsNeeded)
                return MoveResult.Failure($"Not enough {move.Color} cards");
        }
        if (move.Locomotives > 0 && player.Hand.GetValueOrDefault(Map.ColorLocomotive) < move.Locomotives)
            return MoveResult.Failure("Not enough locomotive cards");

        return MoveResult.Success;
    }

    private MoveResult ValidateTunnelPay(State state, State.PlayerInfo player, Move move)
    {
        State.PendingTunnelData pt = state.PendingTunnel!;
        int needColor = pt.ExtraColorCardsRequired;
        int needLoco = pt.ExtraLocomotivesRequired;
        // Player can always substitute locomotives for colour cost.
        int extraLocos = move.ExtraLocomotives;
        int extraColor = (needColor + needLoco) - extraLocos;
        if (extraLocos < needLoco)
            return MoveResult.Failure($"You must spend at least {needLoco} additional locomotive(s)");
        if (extraColor < 0)
            return MoveResult.Failure("Too many locomotives offered");
        if (extraColor > 0)
        {
            if (player.Hand.GetValueOrDefault(pt.ColorUsed) < extraColor)
                return MoveResult.Failure($"Not enough {pt.ColorUsed} cards to complete the tunnel");
        }
        if (extraLocos > 0)
        {
            if (player.Hand.GetValueOrDefault(Map.ColorLocomotive) < extraLocos)
                return MoveResult.Failure("Not enough locomotive cards to complete the tunnel");
        }
        return MoveResult.Success;
    }

    // ---- IGameRules ---------------------------------------------------------

    public void ApplyMove(State state, Move move, string playerId)
    {
        State.PlayerInfo player = state.PlayerStates.First(p => p.PlayerId == playerId);
        Random random = new Random(unchecked(state.GameId.GetHashCode() ^ state.TurnNumber ^ Environment.TickCount));

        switch (move.Kind)
        {
            case "keepTickets":
                ApplyKeepTickets(state, player, move);
                break;
            case "drawDeck":
                ApplyDrawDeck(state, player, random);
                break;
            case "drawFace":
                ApplyDrawFace(state, player, move, random);
                break;
            case "claimRoute":
                ApplyClaimRoute(state, player, move, random);
                break;
            case "drawTickets":
                ApplyDrawTickets(state, player);
                break;
            case "buildStation":
                ApplyBuildStation(state, player, move);
                break;
            case "tunnelPay":
                ApplyTunnelPay(state, player, move);
                break;
            case "tunnelSkip":
                ApplyTunnelSkip(state, player);
                break;
            case "resign":
                ApplyResign(state, player);
                break;
        }
    }

    private void ApplyKeepTickets(State state, State.PlayerInfo player, Move move)
    {
        bool[] keep = move.KeepFlags!;
        List<State.TicketCard> kept = new();
        List<State.TicketCard> discarded = new();
        for (int i = 0; i < player.PendingTickets.Count; i++)
        {
            if (keep[i]) kept.Add(player.PendingTickets[i]);
            else discarded.Add(player.PendingTickets[i]);
        }

        player.Tickets.AddRange(kept);
        state.TicketDiscard.AddRange(discarded);
        player.PendingTickets.Clear();
        player.PendingMinKeep = 0;

        AppendLog(state, player.PlayerId, $"kept {kept.Count} ticket(s), discarded {discarded.Count}");

        if (state.Phase == State.PhaseInit)
        {
            player.HasPickedInitialTickets = true;
            // If every non-resigned player has picked, start play
            if (state.PlayerStates.All(p => p.HasResigned || p.HasPickedInitialTickets))
            {
                StartPlayPhase(state);
            }
            return;
        }

        // PhasePlay: in-game ticket draw — turn advances
        state.PendingAction = null;
        AdvanceTurn(state);
    }

    private void ApplyDrawDeck(State state, State.PlayerInfo player, Random random)
    {
        string card = DrawFromDeck(state, random);
        if (card == "") return;
        player.Hand[card] = player.Hand.GetValueOrDefault(card) + 1;
        AppendLog(state, player.PlayerId, "drew a card from the deck");

        if (state.PendingAction == State.PendingDrawSecondCard)
        {
            state.PendingAction = null;
            AdvanceTurn(state);
        }
        else
        {
            state.PendingAction = State.PendingDrawSecondCard;
        }
    }

    private void ApplyDrawFace(State state, State.PlayerInfo player, Move move, Random random)
    {
        string? card = state.FaceUp[move.FaceIndex];
        if (card == null) return;

        bool isLocomotive = card == Map.ColorLocomotive;
        bool wasFirstDraw = state.PendingAction != State.PendingDrawSecondCard;

        player.Hand[card] = player.Hand.GetValueOrDefault(card) + 1;
        state.FaceUp[move.FaceIndex] = state.TrainDeck.Count > 0 ? PopDeckTop(state) : null;
        AppendLog(state, player.PlayerId, $"took a {card.ToLowerInvariant()} card from the market");

        // Locomotive pulled face-up ends the turn (whether first or second draw — we already block second-draw locomotive in validation)
        if (isLocomotive && wasFirstDraw)
        {
            state.PendingAction = null;
            AdvanceTurn(state);
        }
        else if (state.PendingAction == State.PendingDrawSecondCard)
        {
            state.PendingAction = null;
            AdvanceTurn(state);
        }
        else
        {
            state.PendingAction = State.PendingDrawSecondCard;
        }

        // Apply 3-locomotive reshuffle if necessary (after the slot replacement)
        EnforceLocomotiveReshuffle(state, random);
    }

    private void ApplyClaimRoute(State state, State.PlayerInfo player, Move move, Random random)
    {
        Map.RouteInfo route = Map.RouteById[move.RouteId!];
        int locos = move.Locomotives;
        int colorCount = route.Length - locos;
        string colorUsed = colorCount > 0 ? move.Color! : Map.ColorLocomotive;

        // Spend cards from hand
        if (locos > 0)
        {
            player.Hand[Map.ColorLocomotive] -= locos;
            for (int i = 0; i < locos; i++) state.TrainDiscard.Add(Map.ColorLocomotive);
        }
        if (colorCount > 0)
        {
            player.Hand[colorUsed] -= colorCount;
            for (int i = 0; i < colorCount; i++) state.TrainDiscard.Add(colorUsed);
        }

        if (route.IsTunnel)
        {
            // Reveal three cards
            List<string> revealed = new(3);
            for (int i = 0; i < 3; i++)
            {
                string drawn = DrawFromDeck(state, random);
                if (drawn != "") revealed.Add(drawn);
            }

            int matchColor = revealed.Count(c => c == colorUsed && colorUsed != Map.ColorLocomotive);
            int matchLoco = revealed.Count(c => c == Map.ColorLocomotive);
            // Discard the revealed cards
            state.TrainDiscard.AddRange(revealed);

            int extraColor = matchColor;
            int extraLoco = matchLoco;
            // If both matchColor and matchLoco are zero, no extra cost — finish immediately.
            if (extraColor + extraLoco == 0)
            {
                FinaliseRouteClaim(state, player, route, colorUsed, locos);
                AppendLog(state, player.PlayerId, $"claimed tunnel {route.CityA}-{route.CityB}, no extra cost");
                state.PendingAction = null;
                AdvanceTurn(state);
                return;
            }

            // Set pending tunnel state
            state.PendingTunnel = new State.PendingTunnelData
            {
                PlayerId = player.PlayerId,
                RouteId = route.Id,
                ColorUsed = colorUsed == Map.ColorLocomotive ? "" : colorUsed,
                LocomotivesPaid = locos,
                ColorCardsPaid = colorCount,
                RevealedCards = revealed,
                ExtraColorCardsRequired = extraColor,
                ExtraLocomotivesRequired = extraLoco
            };
            state.PendingAction = State.PendingTunnelDecision;
            AppendLog(state, player.PlayerId, $"is attempting tunnel {route.CityA}-{route.CityB}; {extraColor + extraLoco} extra card(s) needed");
            return;
        }

        FinaliseRouteClaim(state, player, route, colorUsed, locos);
        AppendLog(state, player.PlayerId, $"claimed {route.CityA}-{route.CityB} ({route.Length} cars)");
        state.PendingAction = null;
        AdvanceTurn(state);
    }

    private void FinaliseRouteClaim(State state, State.PlayerInfo player, Map.RouteInfo route,
        string colorUsed, int locomotivesUsed)
    {
        state.ClaimedRoutes.Add(new State.ClaimedRouteInfo
        {
            RouteId = route.Id,
            PlayerId = player.PlayerId,
            ColorUsed = colorUsed,
            LocomotivesUsed = locomotivesUsed
        });

        player.TrainsRemaining -= route.Length;
        int routeScore = Map.RouteLengthScores.TryGetValue(route.Length, out int s) ? s : route.Length;
        player.Score += routeScore;

        if (!state.LastRoundTriggered && player.TrainsRemaining <= 2)
        {
            state.LastRoundTriggered = true;
            state.LastRoundTriggeredBy = player.PlayerId;
            AppendLog(state, "system", $"{player.PlayerId} has {player.TrainsRemaining} train(s) left — final round!");
        }
    }

    private void ApplyDrawTickets(State state, State.PlayerInfo player)
    {
        int draw = Math.Min(DrawnTicketCount, state.TicketDeck.Count);
        for (int i = 0; i < draw; i++)
        {
            State.TicketCard t = state.TicketDeck[0];
            state.TicketDeck.RemoveAt(0);
            player.PendingTickets.Add(t);
        }
        player.PendingMinKeep = Math.Min(DrawnTicketsMustKeep, draw);
        state.PendingAction = State.PendingKeepDrawnTickets;
        AppendLog(state, player.PlayerId, $"drew {draw} destination ticket(s) to choose from");
    }

    private void ApplyBuildStation(State state, State.PlayerInfo player, Move move)
    {
        int cost = player.StationsBuilt + 1;
        if (move.Locomotives > 0)
        {
            player.Hand[Map.ColorLocomotive] -= move.Locomotives;
            for (int i = 0; i < move.Locomotives; i++) state.TrainDiscard.Add(Map.ColorLocomotive);
        }
        int colorPaid = cost - move.Locomotives;
        if (colorPaid > 0)
        {
            player.Hand[move.Color!] -= colorPaid;
            for (int i = 0; i < colorPaid; i++) state.TrainDiscard.Add(move.Color!);
        }
        player.StationsBuilt++;
        state.Stations.Add(new State.StationInfo
        {
            CityId = move.CityId!,
            PlayerId = player.PlayerId
        });
        AppendLog(state, player.PlayerId, $"built a station in {Map.CityById[move.CityId!].Name}");
        state.PendingAction = null;
        AdvanceTurn(state);
    }

    private void ApplyTunnelPay(State state, State.PlayerInfo player, Move move)
    {
        State.PendingTunnelData pt = state.PendingTunnel!;
        int extraColor = (pt.ExtraColorCardsRequired + pt.ExtraLocomotivesRequired) - move.ExtraLocomotives;
        if (move.ExtraLocomotives > 0)
        {
            player.Hand[Map.ColorLocomotive] -= move.ExtraLocomotives;
            for (int i = 0; i < move.ExtraLocomotives; i++) state.TrainDiscard.Add(Map.ColorLocomotive);
        }
        if (extraColor > 0)
        {
            player.Hand[pt.ColorUsed] -= extraColor;
            for (int i = 0; i < extraColor; i++) state.TrainDiscard.Add(pt.ColorUsed);
        }

        Map.RouteInfo route = Map.RouteById[pt.RouteId];
        FinaliseRouteClaim(state, player, route,
            string.IsNullOrEmpty(pt.ColorUsed) ? Map.ColorLocomotive : pt.ColorUsed,
            pt.LocomotivesPaid + move.ExtraLocomotives);

        AppendLog(state, player.PlayerId,
            $"claimed tunnel {route.CityA}-{route.CityB} after paying {move.ExtraLocomotives + extraColor} extra card(s)");

        state.PendingTunnel = null;
        state.PendingAction = null;
        AdvanceTurn(state);
    }

    private void ApplyTunnelSkip(State state, State.PlayerInfo player)
    {
        State.PendingTunnelData pt = state.PendingTunnel!;
        // Return original cards to player's hand
        if (pt.LocomotivesPaid > 0)
        {
            player.Hand[Map.ColorLocomotive] = player.Hand.GetValueOrDefault(Map.ColorLocomotive) + pt.LocomotivesPaid;
            for (int i = 0; i < pt.LocomotivesPaid; i++)
            {
                state.TrainDiscard.RemoveAt(state.TrainDiscard.LastIndexOf(Map.ColorLocomotive));
            }
        }
        if (pt.ColorCardsPaid > 0 && !string.IsNullOrEmpty(pt.ColorUsed))
        {
            player.Hand[pt.ColorUsed] = player.Hand.GetValueOrDefault(pt.ColorUsed) + pt.ColorCardsPaid;
            for (int i = 0; i < pt.ColorCardsPaid; i++)
            {
                state.TrainDiscard.RemoveAt(state.TrainDiscard.LastIndexOf(pt.ColorUsed));
            }
        }

        Map.RouteInfo route = Map.RouteById[pt.RouteId];
        AppendLog(state, player.PlayerId, $"backed out of tunnel {route.CityA}-{route.CityB}");
        state.PendingTunnel = null;
        state.PendingAction = null;
        AdvanceTurn(state);
    }

    // ---- Turn management ----------------------------------------------------

    private void AdvanceTurn(State state)
    {
        // If we are in the final-round countdown, mark current player done.
        if (state.LastRoundTriggered)
        {
            state.PlayerStates[state.CurrentPlayerIndex].HasTakenFinalTurn = true;
            if (state.PlayerStates.All(p => p.HasResigned || p.HasTakenFinalTurn))
            {
                FinaliseGame(state);
                return;
            }
        }

        // If only one (or zero) active players remain, end the game now.
        if (state.PlayerStates.Count(p => !p.HasResigned) <= 1)
        {
            FinaliseGame(state);
            return;
        }

        // Move to next non-resigned player who still needs a turn.
        int safety = 0;
        do
        {
            state.CurrentPlayerIndex = (state.CurrentPlayerIndex + 1) % state.PlayerStates.Count;
            if (state.CurrentPlayerIndex == 0) state.TurnNumber++;
            if (++safety > state.PlayerStates.Count * 2)
            {
                FinaliseGame(state);
                return;
            }
        } while (state.PlayerStates[state.CurrentPlayerIndex].HasResigned ||
                 (state.LastRoundTriggered && state.PlayerStates[state.CurrentPlayerIndex].HasTakenFinalTurn));
    }

    private void StartPlayPhase(State state)
    {
        state.Phase = State.PhasePlay;
        state.TurnNumber = 1;
        state.CurrentPlayerIndex = 0;
        for (int i = 0; i < state.PlayerStates.Count; i++)
        {
            if (!state.PlayerStates[i].HasResigned)
            {
                state.CurrentPlayerIndex = i;
                break;
            }
        }
        AppendLog(state, "system", $"All players ready. {state.PlayerStates[state.CurrentPlayerIndex].PlayerId}'s turn.");
    }

    private void ApplyResign(State state, State.PlayerInfo player)
    {
        bool wasTheirTurn = state.Phase == State.PhasePlay
            && state.PlayerStates[state.CurrentPlayerIndex].PlayerId == player.PlayerId;

        player.HasResigned = true;

        // Discard any pending tickets the player was deciding on.
        if (player.PendingTickets.Count > 0)
        {
            state.TicketDiscard.AddRange(player.PendingTickets);
            player.PendingTickets.Clear();
        }
        player.PendingMinKeep = 0;

        // In init phase, mark them as having "picked" so we no longer wait for them.
        if (state.Phase == State.PhaseInit)
        {
            player.HasPickedInitialTickets = true;
        }

        // If they were mid-tunnel, drop their pending tunnel state (cards already in discard).
        if (state.PendingTunnel != null && state.PendingTunnel.PlayerId == player.PlayerId)
        {
            state.PendingTunnel = null;
            state.PendingAction = null;
        }

        AppendLog(state, player.PlayerId, "resigned");

        // If only one active player remains (or none), end the game immediately.
        if (state.PlayerStates.Count(p => !p.HasResigned) <= 1)
        {
            FinaliseGame(state);
            return;
        }

        // Init phase: if every remaining (non-resigned) player has picked, start play.
        if (state.Phase == State.PhaseInit)
        {
            if (state.PlayerStates.All(p => p.HasResigned || p.HasPickedInitialTickets))
            {
                StartPlayPhase(state);
            }
            return;
        }

        // Play phase: if it was their turn, clear their pending action and move on.
        if (wasTheirTurn)
        {
            state.PendingAction = null;
            AdvanceTurn(state);
        }
    }

    private void FinaliseGame(State state)
    {
        // Score destination tickets and station bonus for non-resigned players only.
        foreach (State.PlayerInfo p in state.PlayerStates)
        {
            if (p.HasResigned) continue;
            HashSet<string> playerCities = ConnectedCitiesFor(state, p);
            foreach (State.TicketCard ticket in p.Tickets)
            {
                bool connected = AreConnected(state, p, ticket.CityA, ticket.CityB, playerCities);
                p.Score += connected ? ticket.Points : -ticket.Points;
            }
            int unused = MaxStations - p.StationsBuilt;
            p.Score += unused * StationBonus;
        }

        // Longest path bonus (resigned players excluded).
        int longest = 0;
        List<State.PlayerInfo> longestHolders = new();
        foreach (State.PlayerInfo p in state.PlayerStates)
        {
            if (p.HasResigned) continue;
            int len = LongestPathLength(state, p);
            if (len > longest)
            {
                longest = len;
                longestHolders = new List<State.PlayerInfo> { p };
            }
            else if (len == longest && len > 0)
            {
                longestHolders.Add(p);
            }
        }
        foreach (State.PlayerInfo p in longestHolders)
        {
            p.Score += LongestPathBonus;
        }

        state.Phase = State.PhaseEnded;
        state.IsFinished = true;
        // Set Winner here too: when ApplyResign triggers FinaliseGame the engine's
        // post-move CheckFinished branch may not run (the move has not yet completed
        // a "round" in the engine's eyes), so Winner would otherwise stay null.
        state.Winner = DetermineWinner(state);
        AppendLog(state, "system", "Game over. Final scores calculated.");
    }

    public bool CheckFinished(State state) => state.Phase == State.PhaseEnded;

    public string? DetermineWinner(State state)
    {
        List<State.PlayerInfo> active = state.PlayerStates.Where(p => !p.HasResigned).ToList();
        if (active.Count == 0) return null;
        // Last player standing always wins regardless of score.
        if (active.Count == 1) return active[0].PlayerId;

        int top = active.Max(p => p.Score);
        List<State.PlayerInfo> winners = active.Where(p => p.Score == top).ToList();
        if (winners.Count == 1) return winners[0].PlayerId;
        // Tiebreaker 1: most completed tickets
        winners = winners
            .GroupBy(p => p.Tickets.Count(t => AreConnected(state, p, t.CityA, t.CityB, ConnectedCitiesFor(state, p))))
            .OrderByDescending(g => g.Key)
            .First()
            .ToList();
        if (winners.Count == 1) return winners[0].PlayerId;
        // Tiebreaker 2: fewest stations used
        winners = winners
            .GroupBy(p => p.StationsBuilt)
            .OrderBy(g => g.Key)
            .First()
            .ToList();
        return winners.Count == 1 ? winners[0].PlayerId : null;
    }

    public IEnumerable<string> GetExpectedPlayers(State state)
    {
        if (state.Phase == State.PhaseEnded) return Enumerable.Empty<string>();
        if (state.PlayerStates.Count == 0) return Enumerable.Empty<string>();

        if (state.Phase == State.PhaseInit)
        {
            return state.PlayerStates
                .Where(p => !p.HasResigned && !p.HasPickedInitialTickets)
                .Select(p => p.PlayerId);
        }

        // Play phase: return all non-resigned players so that any of them can submit
        // a "resign" move at any time. ValidateMove enforces turn order for the
        // game-action moves; only resign is exempt.
        return state.PlayerStates
            .Where(p => !p.HasResigned)
            .Select(p => p.PlayerId);
    }

    public bool NeedToFinish(State state) => false;

    public IReadOnlyDictionary<string, int> GetPlayerScores(State state)
    {
        Dictionary<string, int> scores = new();
        foreach (State.PlayerInfo p in state.PlayerStates)
        {
            // Resigned players forfeit any awarded points.
            scores[p.PlayerId] = p.HasResigned ? 0 : Math.Max(0, p.Score);
        }
        return scores;
    }

    // ---- Deck / face-up helpers --------------------------------------------

    private string DrawFromDeck(State state, Random random)
    {
        if (state.TrainDeck.Count == 0)
        {
            if (state.TrainDiscard.Count == 0) return "";
            // Reshuffle discards into deck
            state.TrainDeck.AddRange(state.TrainDiscard);
            state.TrainDiscard.Clear();
            Shuffle(state.TrainDeck, random);
        }
        return PopDeckTop(state);
    }

    private static string PopDeckTop(State state)
    {
        string c = state.TrainDeck[0];
        state.TrainDeck.RemoveAt(0);
        return c;
    }

    private void RefillFaceUp(State state, Random random)
    {
        for (int i = 0; i < state.FaceUp.Count; i++)
        {
            if (state.FaceUp[i] == null && state.TrainDeck.Count > 0)
            {
                state.FaceUp[i] = PopDeckTop(state);
            }
        }
        EnforceLocomotiveReshuffle(state, random);
    }

    private void EnforceLocomotiveReshuffle(State state, Random random)
    {
        int safety = 0;
        while (safety++ < 5)
        {
            int locos = state.FaceUp.Count(c => c == Map.ColorLocomotive);
            if (locos < LocomotiveReshuffleThreshold) return;

            // Discard all face-up, refill
            foreach (string? c in state.FaceUp)
            {
                if (c != null) state.TrainDiscard.Add(c);
            }
            for (int i = 0; i < state.FaceUp.Count; i++) state.FaceUp[i] = null;

            for (int i = 0; i < state.FaceUp.Count; i++)
            {
                if (state.TrainDeck.Count == 0)
                {
                    if (state.TrainDiscard.Count == 0) break;
                    state.TrainDeck.AddRange(state.TrainDiscard);
                    state.TrainDiscard.Clear();
                    Shuffle(state.TrainDeck, random);
                }
                state.FaceUp[i] = PopDeckTop(state);
            }
        }
    }

    // ---- Path-finding for tickets and longest path -------------------------

    private static HashSet<string> ConnectedCitiesFor(State state, State.PlayerInfo player)
    {
        // Set of cities accessible to this player through their claimed routes (and stations
        // attached to opponent routes are NOT included here — stations contribute via points only).
        HashSet<string> set = new();
        foreach (State.ClaimedRouteInfo claim in state.ClaimedRoutes.Where(c => c.PlayerId == player.PlayerId))
        {
            Map.RouteInfo r = Map.RouteById[claim.RouteId];
            set.Add(r.CityA);
            set.Add(r.CityB);
        }
        return set;
    }

    private static bool AreConnected(State state, State.PlayerInfo player, string cityA, string cityB, HashSet<string> playerCities)
    {
        if (cityA == cityB) return true;
        if (!playerCities.Contains(cityA) || !playerCities.Contains(cityB)) return false;

        Dictionary<string, List<string>> adj = BuildPlayerAdjacency(state, player);
        if (!adj.ContainsKey(cityA)) return false;

        Queue<string> queue = new();
        HashSet<string> visited = new();
        queue.Enqueue(cityA);
        visited.Add(cityA);

        while (queue.Count > 0)
        {
            string current = queue.Dequeue();
            if (current == cityB) return true;
            if (!adj.TryGetValue(current, out List<string>? neighbors)) continue;
            foreach (string n in neighbors)
            {
                if (visited.Add(n)) queue.Enqueue(n);
            }
        }
        return false;
    }

    private static Dictionary<string, List<string>> BuildPlayerAdjacency(State state, State.PlayerInfo player)
    {
        Dictionary<string, List<string>> adj = new();
        foreach (State.ClaimedRouteInfo claim in state.ClaimedRoutes.Where(c => c.PlayerId == player.PlayerId))
        {
            Map.RouteInfo r = Map.RouteById[claim.RouteId];
            if (!adj.TryGetValue(r.CityA, out List<string>? a))
            {
                a = new List<string>();
                adj[r.CityA] = a;
            }
            a.Add(r.CityB);
            if (!adj.TryGetValue(r.CityB, out List<string>? b))
            {
                b = new List<string>();
                adj[r.CityB] = b;
            }
            b.Add(r.CityA);
        }
        return adj;
    }

    private static int LongestPathLength(State state, State.PlayerInfo player)
    {
        // Build edge list (route id -> length) for player
        List<(string a, string b, int length, string id)> edges = new();
        foreach (State.ClaimedRouteInfo claim in state.ClaimedRoutes.Where(c => c.PlayerId == player.PlayerId))
        {
            Map.RouteInfo r = Map.RouteById[claim.RouteId];
            edges.Add((r.CityA, r.CityB, r.Length, r.Id));
        }
        if (edges.Count == 0) return 0;

        Dictionary<string, List<int>> incident = new();
        for (int i = 0; i < edges.Count; i++)
        {
            (string a, string b, _, _) = edges[i];
            if (!incident.TryGetValue(a, out List<int>? la))
            {
                la = new List<int>();
                incident[a] = la;
            }
            la.Add(i);
            if (!incident.TryGetValue(b, out List<int>? lb))
            {
                lb = new List<int>();
                incident[b] = lb;
            }
            lb.Add(i);
        }

        int best = 0;
        bool[] used = new bool[edges.Count];

        foreach (string startCity in incident.Keys)
        {
            best = Math.Max(best, DfsLongest(startCity, incident, edges, used, 0));
        }
        return best;
    }

    private static int DfsLongest(string city, Dictionary<string, List<int>> incident,
        List<(string a, string b, int length, string id)> edges, bool[] used, int currentLength)
    {
        int best = currentLength;
        if (!incident.TryGetValue(city, out List<int>? edgeIds)) return best;
        foreach (int eid in edgeIds)
        {
            if (used[eid]) continue;
            (string a, string b, int len, _) = edges[eid];
            string next = (a == city) ? b : a;
            used[eid] = true;
            best = Math.Max(best, DfsLongest(next, incident, edges, used, currentLength + len));
            used[eid] = false;
        }
        return best;
    }

    // ---- Misc helpers -------------------------------------------------------

    private static void Shuffle<T>(IList<T> list, Random random)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private static void AppendLog(State state, string playerId, string message)
    {
        state.RecentLog.Add(new State.LogEntry
        {
            Time = DateTime.UtcNow,
            PlayerId = playerId,
            Message = message
        });
        if (state.RecentLog.Count > 30)
        {
            state.RecentLog.RemoveRange(0, state.RecentLog.Count - 30);
        }
    }
}
