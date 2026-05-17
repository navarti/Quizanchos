using System.Collections.Immutable;
using Quizanchos.Common.Enums;
using Quizanchos.Quiz.GameLogic;
using Xunit;

namespace Quizanchos.Quiz.Tests;

public class QuizGameLogicTests
{
    private static readonly ImmutableArray<string> TwoPlayers = ImmutableArray.Create("p1", "p2");
    private static readonly ImmutableArray<string> SinglePlayer = ImmutableArray.Create("p1");

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
    public void CreateInitialState_DefaultConstructor_AppliesDefaults()
    {
        var logic = new QuizGameLogic();

        var state = logic.CreateInitialState(Guid.NewGuid(), SinglePlayer);

        Assert.Equal(10, state.TotalCards);
        Assert.Equal(4, state.OptionCount);
        Assert.Equal(30, state.SecondsPerCard);
        Assert.Equal(GameLevel.Easy, state.GameLevel);
    }

    [Fact]
    public void CreateInitialState_StoresGivenGameId()
    {
        var gameId = Guid.NewGuid();
        var logic = new QuizGameLogic();

        var state = logic.CreateInitialState(gameId, SinglePlayer);

        Assert.Equal(gameId, state.GameId);
    }

    [Fact]
    public void CreateInitialState_WithEmptyPlayers_ProducesEmptyScoreMap()
    {
        var logic = new QuizGameLogic();

        var state = logic.CreateInitialState(Guid.NewGuid(), ImmutableArray<string>.Empty);

        Assert.Empty(state.Players);
        Assert.Empty(state.PlayerScores);
    }

    [Fact]
    public void CreateInitialState_StartsNotFinishedWithNullWinner()
    {
        var logic = new QuizGameLogic();

        var state = logic.CreateInitialState(Guid.NewGuid(), TwoPlayers);

        Assert.False(state.IsFinished);
        Assert.Null(state.Winner);
        Assert.False(state.IsTerminatedByTime);
    }

    [Fact]
    public void CreateInitialState_CarriesCategoryAndLevel()
    {
        var categoryId = Guid.NewGuid();
        var logic = new QuizGameLogic(quizCategoryId: categoryId, gameLevel: GameLevel.Hard);

        var state = logic.CreateInitialState(Guid.NewGuid(), SinglePlayer);

        Assert.Equal(categoryId, state.QuizCategoryId);
        Assert.Equal(GameLevel.Hard, state.GameLevel);
    }

    [Fact]
    public void ValidateMove_NoCurrentCard_ReturnsFailure()
    {
        var logic = new QuizGameLogic();
        var state = logic.CreateInitialState(Guid.NewGuid(), TwoPlayers);

        var result = logic.ValidateMove(state, new QuizMove(0), "p1");

        Assert.False(result.IsSuccess);
        Assert.Contains("No current card", result.Reason);
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
        state.Cards[0].PlayerAnswers["p1"] = 1;

        var result = logic.ValidateMove(state, new QuizMove(2), "p1");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void ValidateMove_AcceptsValidMove()
    {
        var logic = new QuizGameLogic(totalCards: 1);
        var state = NewStateWithCard();

        var result = logic.ValidateMove(state, new QuizMove(0), "p1");

        Assert.True(result.IsSuccess);
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
    public void ApplyMove_RecordsPlayerAnswer()
    {
        var logic = new QuizGameLogic(totalCards: 2);
        var state = NewStateWithCard(correctOption: 2, totalCards: 2);

        logic.ApplyMove(state, new QuizMove(3), "p1");

        Assert.Equal(3, state.Cards[0].PlayerAnswers["p1"]);
        Assert.Equal(3, state.Cards[0].OptionPicked);
    }

    [Fact]
    public void ApplyMove_DoesNotAffectOtherPlayersScore()
    {
        var logic = new QuizGameLogic(totalCards: 2);
        var state = NewStateWithCard(correctOption: 2, totalCards: 2);

        logic.ApplyMove(state, new QuizMove(2), "p1");

        Assert.Equal(0, state.PlayerScores["p2"]);
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
    public void CheckFinished_WhenCardsRemain_ReturnsFalse()
    {
        var logic = new QuizGameLogic(totalCards: 5);
        var state = NewStateWithCard(totalCards: 5);

        Assert.False(logic.CheckFinished(state));
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
    public void DetermineWinner_NoPlayers_ReturnsNull()
    {
        var logic = new QuizGameLogic();
        var state = logic.CreateInitialState(Guid.NewGuid(), ImmutableArray<string>.Empty);

        Assert.Null(logic.DetermineWinner(state));
    }

    [Fact]
    public void DetermineWinner_AllZeroScores_ReturnsNullDraw()
    {
        var logic = new QuizGameLogic();
        var state = logic.CreateInitialState(Guid.NewGuid(), TwoPlayers);

        Assert.Null(logic.DetermineWinner(state));
    }

    [Fact]
    public void NeedToFinish_OnExpiredCard_ReturnsTrue()
    {
        var logic = new QuizGameLogic(totalCards: 1, secondsPerCard: 1);
        var state = logic.CreateInitialState(Guid.NewGuid(), TwoPlayers);
        logic.AddCard(state, Guid.NewGuid(), correctOption: 0, [], [], []);
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

    [Fact]
    public void NeedToFinish_NoCurrentCard_ReturnsFalse()
    {
        var logic = new QuizGameLogic();
        var state = logic.CreateInitialState(Guid.NewGuid(), TwoPlayers);

        Assert.False(logic.NeedToFinish(state));
    }

    [Fact]
    public void GetExpectedPlayers_ReturnsThoseYetToAnswer()
    {
        var logic = new QuizGameLogic(totalCards: 1);
        var state = NewStateWithCard();
        state.Cards[0].PlayerAnswers["p1"] = 1;

        var expected = logic.GetExpectedPlayers(state).ToList();

        Assert.Single(expected);
        Assert.Equal("p2", expected[0]);
    }

    [Fact]
    public void GetExpectedPlayers_NoCurrentCard_ReturnsEmpty()
    {
        var logic = new QuizGameLogic();
        var state = logic.CreateInitialState(Guid.NewGuid(), TwoPlayers);

        Assert.Empty(logic.GetExpectedPlayers(state));
    }

    [Fact]
    public void GetExpectedPlayers_AllAnswered_ReturnsEmpty()
    {
        var logic = new QuizGameLogic(totalCards: 1);
        var state = NewStateWithCard();
        state.Cards[0].PlayerAnswers["p1"] = 1;
        state.Cards[0].PlayerAnswers["p2"] = 1;

        Assert.Empty(logic.GetExpectedPlayers(state));
    }

    [Fact]
    public void GetPlayerScores_AppliesCoefficientBasedOnOptionCount()
    {
        // Coefficient = OptionCount / 2.0. For OptionCount=4 -> 2.0x
        var logic = new QuizGameLogic(totalCards: 5, optionCount: 4);
        var state = logic.CreateInitialState(Guid.NewGuid(), TwoPlayers);
        state.PlayerScores["p1"] = 3;
        state.PlayerScores["p2"] = 1;

        var scores = logic.GetPlayerScores(state);

        Assert.Equal(6, scores["p1"]);
        Assert.Equal(2, scores["p2"]);
    }

    [Fact]
    public void GetPlayerScores_TwoOptions_HasUnityCoefficient()
    {
        var logic = new QuizGameLogic(totalCards: 5, optionCount: 2);
        var state = logic.CreateInitialState(Guid.NewGuid(), SinglePlayer);
        state.PlayerScores["p1"] = 4;

        var scores = logic.GetPlayerScores(state);

        Assert.Equal(4, scores["p1"]);
    }

    [Fact]
    public void GetPlayerScores_SixOptions_TriplesScore()
    {
        var logic = new QuizGameLogic(totalCards: 5, optionCount: 6);
        var state = logic.CreateInitialState(Guid.NewGuid(), SinglePlayer);
        state.PlayerScores["p1"] = 2;

        var scores = logic.GetPlayerScores(state);

        Assert.Equal(6, scores["p1"]);
    }

    [Fact]
    public void AddCard_AdvancesIndexAndSetsCardIndexAccordingly()
    {
        var logic = new QuizGameLogic(totalCards: 3);
        var state = logic.CreateInitialState(Guid.NewGuid(), TwoPlayers);

        logic.AddCard(state, Guid.NewGuid(), 1, [], [], []);
        logic.AddCard(state, Guid.NewGuid(), 0, [], [], []);

        Assert.Equal(1, state.CurrentCardIndex);
        Assert.Equal(2, state.Cards.Count);
        Assert.Equal(0, state.Cards[0].CardIndex);
        Assert.Equal(1, state.Cards[1].CardIndex);
    }

    [Fact]
    public void AddCard_PreservesEntityData()
    {
        var logic = new QuizGameLogic();
        var state = logic.CreateInitialState(Guid.NewGuid(), SinglePlayer);
        var entityId = Guid.NewGuid();

        logic.AddCard(state, Guid.NewGuid(), correctOption: 2,
            entityIds: [entityId],
            entityNames: ["Apple"],
            optionValues: [42]);

        var card = state.Cards[0];
        Assert.Equal(2, card.CorrectOption);
        Assert.Equal(entityId, card.EntityIds[0]);
        Assert.Equal("Apple", card.EntityNames[0]);
        Assert.Equal(42, card.OptionValues[0]);
    }
}
