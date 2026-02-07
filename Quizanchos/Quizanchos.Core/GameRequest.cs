using System.Text.Json.Serialization;

namespace Quizanchos.Core;

public record GameRequest
{
    [JsonPropertyName("gameId")]
    public Guid GameId { get; init; }
    
    [JsonPropertyName("move")]
    public GameMove Move { get; init; } = null!;
}
