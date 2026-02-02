using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Quizanchos.Domain;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Entities.Quiz;
using Quizanchos.Domain.Repositories.Quiz.Interfaces;
using Quizanchos.Quiz.GameLogic;

namespace Quizanchos.Quiz.Services;

public class QuizGameStateService
{
    private readonly QuizanchosDbContext _context;
    private readonly IQuizGameSessionRepository _repository;

    public QuizGameStateService(QuizanchosDbContext context, IQuizGameSessionRepository repository)
    {
        _context = context;
        _repository = repository;
    }

    public async Task<QuizGameState?> LoadStateAsync(Guid gameSessionId)
    {
        QuizGameSessionState? sessionState = await _repository.GetByGameSessionIdAsync(gameSessionId);
        if (sessionState == null)
            return null;

        return ConvertToGameState(sessionState);
    }

    public async Task SaveStateAsync(Guid gameSessionId, QuizGameState state)
    {
        QuizGameSessionState? existingState = await _repository.GetByGameSessionIdAsync(gameSessionId);
        
        if (existingState == null)
        {
            throw new InvalidOperationException($"QuizGameSessionState not found for GameSessionId: {gameSessionId}");
        }

        // Update the state
        existingState.CurrentCardIndex = state.CurrentCardIndex;
        existingState.IsTerminatedByTime = state.IsTerminatedByTime;

        // Update game session
        existingState.GameSession.IsFinished = state.IsFinished;
        if (state.Winner.HasValue)
        {
            existingState.GameSession.WinnerId = state.Winner.Value.ToString();
            existingState.GameSession.FinishedAt = DateTime.UtcNow;
        }
        if (state.IsFinished)
        {
            existingState.GameSession.IsActive = false;
        }

        // Update cards
        foreach (var card in state.Cards)
        {
            QuizSessionCard? existingCard = existingState.Cards.FirstOrDefault(c => c.CardIndex == card.CardIndex);
            if (existingCard == null)
            {
                existingCard = new QuizSessionCard
                {
                    Id = card.Id,
                    QuizGameSessionStateId = existingState.Id,
                    CardIndex = card.CardIndex,
                    CorrectOption = card.CorrectOption,
                    OptionPicked = card.OptionPicked,
                    CreationTime = card.CreationTime,
                    EntityIdsJson = JsonSerializer.Serialize(card.EntityIds),
                    EntityNamesJson = JsonSerializer.Serialize(card.EntityNames),
                    OptionValuesJson = JsonSerializer.Serialize(card.OptionValues)
                };
                existingState.Cards.Add(existingCard);
            }
            else
            {
                existingCard.OptionPicked = card.OptionPicked;
            }

            // Update player answers
            foreach (var answer in card.PlayerAnswers)
            {
                QuizSessionCardAnswer? existingAnswer = existingCard.PlayerAnswers
                    .FirstOrDefault(a => a.ApplicationUserId == answer.Key.ToString());
                
                if (existingAnswer == null && answer.Value.HasValue)
                {
                    existingCard.PlayerAnswers.Add(new QuizSessionCardAnswer
                    {
                        Id = Guid.NewGuid(),
                        QuizSessionCardId = existingCard.Id,
                        ApplicationUserId = answer.Key.ToString(),
                        OptionPicked = answer.Value,
                        AnsweredAt = DateTime.UtcNow
                    });
                }
                else if (existingAnswer != null)
                {
                    existingAnswer.OptionPicked = answer.Value;
                }
            }
        }

        // Update player scores
        foreach (var score in state.PlayerScores)
        {
            QuizSessionPlayerScore? existingScore = existingState.PlayerScores
                .FirstOrDefault(s => s.ApplicationUserId == score.Key.ToString());
            
            if (existingScore != null)
            {
                existingScore.Score = score.Value;
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task<QuizGameSessionState> CreateInitialStateAsync(
        GameSession gameSession,
        Guid quizCategoryId,
        int totalCards,
        int gameLevel,
        int secondsPerCard,
        int optionCount)
    {
        QuizGameSessionState sessionState = new QuizGameSessionState
        {
            Id = Guid.NewGuid(),
            GameSessionId = gameSession.Id,
            QuizCategoryId = quizCategoryId,
            GameLevel = (Common.Enums.GameLevel)gameLevel,
            SecondsPerCard = secondsPerCard,
            OptionCount = optionCount,
            TotalCards = totalCards,
            CurrentCardIndex = -1,
            IsTerminatedByTime = false,
            CreationTime = DateTime.UtcNow
        };

        // Create player scores
        foreach (var player in gameSession.Players)
        {
            sessionState.PlayerScores.Add(new QuizSessionPlayerScore
            {
                Id = Guid.NewGuid(),
                QuizGameSessionStateId = sessionState.Id,
                ApplicationUserId = player.ApplicationUserId,
                Score = 0
            });
        }

        await _repository.CreateAsync(sessionState);
        return sessionState;
    }

    private QuizGameState ConvertToGameState(QuizGameSessionState sessionState)
    {
        QuizGameState state = new QuizGameState
        {
            GameId = sessionState.GameSessionId,
            Players = sessionState.GameSession.Players.Select(p => Guid.Parse(p.ApplicationUserId)).ToList(),
            IsFinished = sessionState.GameSession.IsFinished,
            Winner = sessionState.GameSession.WinnerId != null ? Guid.Parse(sessionState.GameSession.WinnerId) : null,
            CurrentCardIndex = sessionState.CurrentCardIndex,
            TotalCards = sessionState.TotalCards,
            QuizCategoryId = sessionState.QuizCategoryId,
            GameLevel = sessionState.GameLevel,
            SecondsPerCard = sessionState.SecondsPerCard,
            OptionCount = sessionState.OptionCount,
            CreationTime = sessionState.CreationTime,
            IsTerminatedByTime = sessionState.IsTerminatedByTime
        };

        // Convert player scores
        foreach (var score in sessionState.PlayerScores)
        {
            state.PlayerScores[Guid.Parse(score.ApplicationUserId)] = score.Score;
        }

        // Convert cards
        foreach (var card in sessionState.Cards.OrderBy(c => c.CardIndex))
        {
            var gameCard = new QuizGameState.QuizCard
            {
                Id = card.Id,
                CardIndex = card.CardIndex,
                CorrectOption = card.CorrectOption,
                OptionPicked = card.OptionPicked,
                CreationTime = card.CreationTime,
                EntityIds = JsonSerializer.Deserialize<Guid[]>(card.EntityIdsJson) ?? Array.Empty<Guid>(),
                EntityNames = JsonSerializer.Deserialize<string[]>(card.EntityNamesJson) ?? Array.Empty<string>(),
                OptionValues = JsonSerializer.Deserialize<object[]>(card.OptionValuesJson) ?? Array.Empty<object>()
            };

            foreach (var answer in card.PlayerAnswers)
            {
                gameCard.PlayerAnswers[Guid.Parse(answer.ApplicationUserId)] = answer.OptionPicked;
            }

            state.Cards.Add(gameCard);
        }

        return state;
    }
}
