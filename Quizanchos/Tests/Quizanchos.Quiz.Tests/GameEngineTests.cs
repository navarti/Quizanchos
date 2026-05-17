using System.Collections.Immutable;
using Quizanchos.Core;
using Quizanchos.Quiz.GameLogic;
using Xunit;

namespace Quizanchos.Quiz.Tests;

public class GameEngineTests
{
    private static readonly ImmutableArray<string> SinglePlayer = ImmutableArray.Create("p1");
    private static readonly ImmutableArray<string> TwoPlayers = ImmutableArray.Create("p1", "p2");

    private static (GameEngine<QuizGameState, QuizMove> engine, QuizGameLogic logic) NewEngineWithCard(
        int totalCards = 2,
        int correctOption = 1,
        ImmutableArray<string>? players = null)
    {
        var logic = new QuizGameLogic(totalCards: totalCards);
        var engine = new GameEngine<QuizGameState, QuizMove>(logic, Guid.NewGuid(), players ?? SinglePlayer);
        logic.AddCard(engine.State, Guid.NewGuid(), correctOption, [], [], []);
        return (engine, logic);
    }

    [Fact]
    public void Constructor_WithPlayers_InitializesState()
    {
        var logic = new QuizGameLogic(totalCards: 5);
        var gameId = Guid.NewGuid();

        var engine = new GameEngine<QuizGameState, QuizMove>(logic, gameId, TwoPlayers);

        Assert.Equal(gameId, engine.State.GameId);
        Assert.Equal(2, engine.State.Players.Count);
        Assert.False(engine.State.IsFinished);
    }

    [Fact]
    public void Constructor_WithExistingState_UsesProvidedState()
    {
        var logic = new QuizGameLogic();
        var existing = logic.CreateInitialState(Guid.NewGuid(), SinglePlayer);
        existing.PlayerScores["p1"] = 42;

        var engine = new GameEngine<QuizGameState, QuizMove>(logic, existing);

        Assert.Same(existing, engine.State);
        Assert.Equal(42, engine.State.PlayerScores["p1"]);
    }

    [Fact]
    public void MakeMove_OnFinishedGame_Fails()
    {
        var (engine, _) = NewEngineWithCard();
        engine.State.IsFinished = true;

        var result = engine.MakeMove("p1", new QuizMove(1));

        Assert.False(result.IsSuccess);
        Assert.Equal("Game is finished", result.Reason);
    }

    [Fact]
    public void MakeMove_UnexpectedPlayer_Fails()
    {
        var (engine, _) = NewEngineWithCard();

        var result = engine.MakeMove("intruder", new QuizMove(1));

        Assert.False(result.IsSuccess);
        Assert.Contains("can not commit", result.Reason);
    }

    [Fact]
    public void MakeMove_InvalidMove_ReturnsValidationFailure()
    {
        var (engine, _) = NewEngineWithCard();

        var result = engine.MakeMove("p1", new QuizMove(-1));

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void MakeMove_ValidMove_ReturnsSuccess()
    {
        var (engine, _) = NewEngineWithCard(totalCards: 2);

        var result = engine.MakeMove("p1", new QuizMove(1));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void MakeMove_FinishesGame_AfterAllCardsAnswered()
    {
        var (engine, _) = NewEngineWithCard(totalCards: 1, correctOption: 1);

        var result = engine.MakeMove("p1", new QuizMove(1));

        Assert.True(result.IsSuccess);
        Assert.True(engine.State.IsFinished);
        Assert.Equal("p1", engine.State.Winner);
    }

    [Fact]
    public void NeedToFinish_FinishedGame_ReturnsFalse()
    {
        var (engine, _) = NewEngineWithCard();
        engine.State.IsFinished = true;

        Assert.False(engine.NeedToFinish());
    }

    [Fact]
    public void GetPlayerScores_DelegatesToLogic()
    {
        var (engine, _) = NewEngineWithCard(totalCards: 1, correctOption: 1);
        engine.MakeMove("p1", new QuizMove(1));

        var scores = engine.GetPlayerScores();

        // OptionCount default = 4, coefficient = 2.0
        Assert.Equal(2, scores["p1"]);
    }
}
