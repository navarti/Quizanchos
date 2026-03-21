using Quizanchos.Common.Enums;
using Quizanchos.Core;

namespace Quizanchos.Game2048.Descriptors;

public class Game2048FrontendDescriptor : IMinigameFrontendDescriptor
{
    public int MinigameTypeId => 2;
    public string GameKey => "Game2048";
    public string DisplayName => "2048";
    public string Description => "Join the tiles and reach 2048.";
    public string CardStyle => "background: linear-gradient(135deg, #edc22e 0%, #f2b179 100%);";
    public string LobbyUrl => "/Game2048";
    public string GameUrlTemplate => "/Game2048/{gameId}";
    public string ActionText => "PLAY →";
    public int Order => 2;
}
