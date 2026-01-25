using System.Text.Json.Serialization;

namespace Quizanchos.Core;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "gameType")]
[JsonDerivedType(typeof(GameMove), "base")]
public abstract record GameMove
{
}
