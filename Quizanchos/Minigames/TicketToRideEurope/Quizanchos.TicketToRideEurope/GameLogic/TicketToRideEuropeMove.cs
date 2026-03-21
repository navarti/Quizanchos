using Quizanchos.Core;
using System.Text.Json.Serialization;

namespace Quizanchos.TicketToRideEurope.GameLogic;

[JsonDerivedType(typeof(TicketToRideEuropeMove), "ticketToRideEurope")]
public record TicketToRideEuropeMove : GameMove
{
    [JsonPropertyName("action")]
    public TicketToRideEuropeAction Action { get; init; }

    [JsonPropertyName("drawSources")]
    public List<string>? DrawSources { get; init; }

    [JsonPropertyName("routeId")]
    public string? RouteId { get; init; }

    [JsonPropertyName("color")]
    public string? Color { get; init; }

    [JsonPropertyName("keepTicketIndexes")]
    public List<int>? KeepTicketIndexes { get; init; }

    [JsonPropertyName("stationCity")]
    public string? StationCity { get; init; }

    public TicketToRideEuropeMove() { }
}

public enum TicketToRideEuropeAction
{
    DrawCards = 0,
    ClaimRoute = 1,
    DrawDestinationTickets = 2,
    KeepDestinationTickets = 3,
    PlaceStation = 4
}
