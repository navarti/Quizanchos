using Quizanchos.Common.Enums;
using Quizanchos.Core;

namespace Quizanchos.QuizMultiplayer.Descriptors;

public class QuizMultiplayerFrontendDescriptor : IMinigameFrontendDescriptor
{
    public int MinigameTypeId => 3;
    public string GameKey => "QuizMultiplayer";
    public string DisplayName => "Quiz Multiplayer";
    public string Description => "Team up and compete in real-time quizzes.";
    public string CardStyle => "background: linear-gradient(135deg, #7b61ff 0%, #4fc3f7 100%);";
    public string LobbyUrl => "/QuizMultiplayer";
    public string GameUrlTemplate => "/QuizMultiplayer/{gameId}";
    public string ActionText => "FIND ROOM →";
    public int Order => 3;
}
