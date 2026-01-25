using Quizanchos.Core;
using System.Text.Json.Serialization;

namespace Quizanchos.Quiz.GameLogic;

[JsonDerivedType(typeof(QuizMove), "quiz")]
public record QuizMove : GameMove
{
    [JsonPropertyName("optionPicked")]
    public int OptionPicked { get; init; }

    public QuizMove(int optionPicked)
    {
        OptionPicked = optionPicked;
    }

    public QuizMove() : this(0) { }
}
