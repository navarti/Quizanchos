using Quizanchos.Core;
using System.Text.Json.Serialization;

namespace Quizanchos.Game2048.GameLogic;

[JsonDerivedType(typeof(Game2048Move), "game2048")]
public record Game2048Move : GameMove
{
    [JsonPropertyName("direction")]
    public Direction Direction { get; init; }

    public Game2048Move(Direction direction)
    {
        Direction = direction;
    }

    public Game2048Move() : this(Direction.Up) { }
}

public enum Direction
{
    Up = 0,
    Down = 1,
    Left = 2,
    Right = 3
}
