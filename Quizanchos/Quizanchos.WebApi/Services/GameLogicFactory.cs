using Quizanchos.Common.Enums;
using Quizanchos.Core;
using Quizanchos.Quiz.GameLogic;
using System.Collections.Immutable;

namespace Quizanchos.WebApi.Services;

public class GameLogicFactory : IGameLogicFactory
{
    public object CreateGameEngine(MinigameType type, Guid gameId, ImmutableArray<Guid> playerIds, Dictionary<string, object> parameters)
    {
        return type switch
        {
            MinigameType.Quiz => CreateQuizEngine(gameId, playerIds, parameters),
            _ => throw new ArgumentException($"Unknown minigame type: {type}")
        };
    }

    public Type GetStateType(MinigameType type)
    {
        return type switch
        {
            MinigameType.Quiz => typeof(QuizGameState),
            _ => throw new ArgumentException($"Unknown minigame type: {type}")
        };
    }

    public Type GetMoveType(MinigameType type)
    {
        return type switch
        {
            MinigameType.Quiz => typeof(QuizMove),
            _ => throw new ArgumentException($"Unknown minigame type: {type}")
        };
    }

    private GameEngine<QuizGameState, QuizMove> CreateQuizEngine(Guid gameId, ImmutableArray<Guid> playerIds, Dictionary<string, object> parameters)
    {
        int totalCards = parameters.TryGetValue("totalCards", out object? value) && value is int cards ? cards : 10;
        QuizGameLogic logic = new QuizGameLogic(totalCards);
        return new GameEngine<QuizGameState, QuizMove>(logic, gameId, playerIds);
    }
}
