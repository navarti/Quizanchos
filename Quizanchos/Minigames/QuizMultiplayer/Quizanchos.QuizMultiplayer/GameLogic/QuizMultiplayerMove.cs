using Quizanchos.Core;
using System.Text.Json.Serialization;

namespace Quizanchos.QuizMultiplayer.GameLogic;

[JsonDerivedType(typeof(QuizMultiplayerMove), "quizMultiplayer")]
public record QuizMultiplayerMove : GameMove
{
    [JsonPropertyName("optionPicked")]
    public int OptionPicked { get; init; }

    public QuizMultiplayerMove(int optionPicked)
    {
        OptionPicked = optionPicked;
    }

    public QuizMultiplayerMove() : this(0) { }
}
