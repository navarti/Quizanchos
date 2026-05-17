using System.Collections.Immutable;
using Quizanchos.Core;
using Quizanchos.Quiz.GameLogic;
using Xunit;

namespace Quizanchos.Quiz.Tests;

public class GameEngineWrapperTests
{
    private static readonly ImmutableArray<string> SinglePlayer = ImmutableArray.Create("p1");

    private record AlienMove : GameMove;

    private static GameEngineWrapper<QuizGameState, QuizMove> NewWrapper(int correctOption = 1)
    {
        var logic = new QuizGameLogic(totalCards: 1);
        var engine = new GameEngine<QuizGameState, QuizMove>(logic, Guid.NewGuid(), SinglePlayer);
        logic.AddCard(engine.State, Guid.NewGuid(), correctOption, [], [], []);
        return new GameEngineWrapper<QuizGameState, QuizMove>(engine);
    }

    [Fact]
    public void Constructor_NullEngine_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new GameEngineWrapper<QuizGameState, QuizMove>(null!));
    }

    [Fact]
    public void Properties_DelegateToUnderlyingState()
    {
        var wrapper = NewWrapper();

        Assert.NotEqual(Guid.Empty, wrapper.GameId);
        Assert.Single(wrapper.Players);
        Assert.False(wrapper.IsFinished);
        Assert.Null(wrapper.Winner);
    }

    [Fact]
    public void MakeMove_WrongMoveType_ReturnsFailure()
    {
        var wrapper = NewWrapper();

        var result = wrapper.MakeMove("p1", new AlienMove());

        Assert.False(result.IsSuccess);
        Assert.Contains("Invalid move type", result.Reason);
    }

    [Fact]
    public void MakeMove_CorrectMoveType_Succeeds()
    {
        var wrapper = NewWrapper(correctOption: 1);

        var result = wrapper.MakeMove("p1", new QuizMove(1));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void GetState_ReturnsUnderlyingState()
    {
        var wrapper = NewWrapper();

        var state = wrapper.GetState();

        Assert.IsAssignableFrom<QuizGameState>(state);
    }
}
