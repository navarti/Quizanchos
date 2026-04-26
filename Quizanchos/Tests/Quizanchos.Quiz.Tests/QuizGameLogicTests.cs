using System.Collections.Immutable;
using Quizanchos.Common.Enums;
using Quizanchos.Quiz.GameLogic;
using Xunit;

namespace Quizanchos.Quiz.Tests;

public class QuizGameLogicTests
{
    private static readonly ImmutableArray<string> TwoPlayers = ImmutableArray.Create("p1", "p2");

    private static QuizGameState NewStateWithCard(int correctOption = 1, int optionCount = 4, int totalCards = 1)
    {
        var logic = new QuizGameLogic(totalCards: totalCards, optionCount: optionCount);
        var state = logic.CreateInitialState(Guid.NewGuid(), TwoPlayers);
        logic.AddCard(state, Guid.NewGuid(), correctOption, [], [], []);
        return state;
    }

    [Fact]
    public void CreateInitialState_SeedsScoresAtZero_AndCarriesConfig()
    {
        var logic = new QuizGameLogic(totalCards: 7, optionCount: 4, secondsPerCard: 15);

        var state = logic.CreateInitialState(Guid.NewGuid(), TwoPlayers);

        Assert.Equal(7, state.TotalCards);
        Assert.Equal(4, state.OptionCount);
        Assert.Equal(15, state.SecondsPerCard);
        Assert.Equal(2, state.PlayerScores.Count);
        Assert.All(state.PlayerScores.Values, score => Assert.Equal(0, score));
        Assert.False(state.IsFinished);
        Assert.Equal(-1, state.CurrentCardIndex);
    }

    [Fact]
    public void ValidateMove_RejectsNegativeOption()
    {
        var logic = new QuizGameLogic(totalCards: 1);
        var state = NewStateWithCard();

        var result = logic.ValidateMove(state, new QuizMove(-1), "p1");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void ValidateMove_RejectsDoubleAnswerFromSamePlayer()
    {
        var logic = new QuizGameLogic(totalCards: 2);
        var state = NewStateWithCard(correctOption: 1, totalCards: 2);
        // Apply once; current-card pointer must stay on this card for the second move check
        // so we tweak state to simulate "p1 already answered" without advancing the pointer.
        state.Cards[0].PlayerAnswers["p1"] = 1;

        var result = logic.ValidateMove(state, new QuizMove(2), "p1");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void ApplyMove_CorrectAnswer_IncrementsScoreAndAdvancesCard()
    {
        var logic = new QuizGameLogic(totalCards: 2);
        var state = NewStateWithCard(correctOption: 2, totalCards: 2);

        logic.ApplyMove(state, new QuizMove(2), "p1");

        Assert.Equal(1, state.PlayerScores["p1"]);
        Assert.Equal(0, state.PlayerScores["p2"]);
        Assert.Equal(1, state.CurrentCardIndex);
    }

    [Fact]
    public void ApplyMove_WrongAnswer_DoesNotIncrementScore()
    {
        var logic = new QuizGameLogic(totalCards: 2);
        var state = NewStateWithCard(correctOption: 2, totalCards: 2);

        logic.ApplyMove(state, new QuizMove(0), "p1");

        Assert.Equal(0, state.PlayerScores["p1"]);
    }

    [Fact]
    public void CheckFinished_AfterAllCardsAnswered_ReturnsTrue()
    {
        var logic = new QuizGameLogic(totalCards: 1);
        var state = NewStateWithCard(totalCards: 1);

        logic.ApplyMove(state, new QuizMove(0), "p1");

        Assert.True(logic.CheckFinished(state));
    }

    [Fact]
    public void DetermineWinner_OnDraw_ReturnsNull()
    {
        var logic = new QuizGameLogic(totalCards: 1);
        var state = NewStateWithCard(totalCards: 1);
        state.PlayerScores["p1"] = 5;
        state.PlayerScores["p2"] = 5;

        Assert.Null(logic.DetermineWinner(state));
    }

    [Fact]
    public void DetermineWinner_OnUniqueTopScore_ReturnsThatPlayer()
    {
        var logic = new QuizGameLogic(totalCards: 1);
        var state = NewStateWithCard(totalCards: 1);
        state.PlayerScores["p1"] = 7;
        state.PlayerScores["p2"] = 3;

        Assert.Equal("p1", logic.DetermineWinner(state));
    }

    [Fact]
    public void NeedToFinish_OnExpiredCard_ReturnsTrue()
    {
        var logic = new QuizGameLogic(totalCards: 1, secondsPerCard: 1);
        var state = logic.CreateInitialState(Guid.NewGuid(), TwoPlayers);
        logic.AddCard(state, Guid.NewGuid(), correctOption: 0, [], [], []);
        // Backdate creation time to force expiry.
        state.Cards[0].CreationTime = DateTime.UtcNow.AddSeconds(-30);

        Assert.True(logic.NeedToFinish(state));
    }

    [Fact]
    public void NeedToFinish_WhenCardFresh_ReturnsFalse()
    {
        var logic = new QuizGameLogic(totalCards: 1, secondsPerCard: 60);
        var state = logic.CreateInitialState(Guid.NewGuid(), TwoPlayers);
        logic.AddCard(state, Guid.NewGuid(), correctOption: 0, [], [], []);

        Assert.False(logic.NeedToFinish(state));
    }
}
