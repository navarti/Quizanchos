using Quizanchos.Core;
using System.Text.Json.Serialization;

namespace Quizanchos.TicketToRideEurope.GameLogic;

[JsonDerivedType(typeof(TicketToRideEuropeMove), "ticketToRideEurope")]
public record TicketToRideEuropeMove : GameMove
{
    [JsonPropertyName("kind")]
    public string Kind { get; init; } = "";

    [JsonPropertyName("faceIndex")]
    public int FaceIndex { get; init; }

    [JsonPropertyName("routeId")]
    public string? RouteId { get; init; }

    [JsonPropertyName("color")]
    public string? Color { get; init; }

    [JsonPropertyName("locomotives")]
    public int Locomotives { get; init; }

    [JsonPropertyName("keepFlags")]
    public bool[]? KeepFlags { get; init; }

    [JsonPropertyName("cityId")]
    public string? CityId { get; init; }

    [JsonPropertyName("extraLocomotives")]
    public int ExtraLocomotives { get; init; }
}
