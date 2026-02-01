using Microsoft.EntityFrameworkCore;
using Quizanchos.Quiz.Entities;
using Quizanchos.Quiz.Repositories.Interfaces;

namespace Quizanchos.Quiz.Repositories.Realizations;

public class QuizGameSessionRepository : IQuizGameSessionRepository
{
    private readonly QuizDbContext _context;

    public QuizGameSessionRepository(QuizDbContext context)
    {
        _context = context;
    }

    public async Task<QuizGameSessionState?> GetByGameSessionIdAsync(Guid gameSessionId)
    {
        return await _context.QuizGameSessionStates
            .Include(x => x.GameSession)
                .ThenInclude(x => x.Players)
            .Include(x => x.Cards)
                .ThenInclude(x => x.PlayerAnswers)
            .Include(x => x.PlayerScores)
            .FirstOrDefaultAsync(x => x.GameSessionId == gameSessionId);
    }

    public async Task<QuizGameSessionState> CreateAsync(QuizGameSessionState state)
    {
        _context.QuizGameSessionStates.Add(state);
        await _context.SaveChangesAsync();
        return state;
    }

    public async Task UpdateAsync(QuizGameSessionState state)
    {
        _context.QuizGameSessionStates.Update(state);
        await _context.SaveChangesAsync();
    }

    public async Task<QuizSessionCard?> GetCardByIndexAsync(Guid quizGameSessionStateId, int cardIndex)
    {
        return await _context.QuizSessionCards
            .Include(x => x.PlayerAnswers)
            .FirstOrDefaultAsync(x => x.QuizGameSessionStateId == quizGameSessionStateId && x.CardIndex == cardIndex);
    }

    public async Task AddCardAsync(QuizSessionCard card)
    {
        _context.QuizSessionCards.Add(card);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateCardAsync(QuizSessionCard card)
    {
        _context.QuizSessionCards.Update(card);
        await _context.SaveChangesAsync();
    }

    public async Task AddCardAnswerAsync(QuizSessionCardAnswer answer)
    {
        _context.QuizSessionCardAnswers.Add(answer);
        await _context.SaveChangesAsync();
    }

    public async Task UpdatePlayerScoreAsync(Guid quizGameSessionStateId, string applicationUserId, int score)
    {
        QuizSessionPlayerScore? playerScore = await _context.QuizSessionPlayerScores
            .FirstOrDefaultAsync(x => x.QuizGameSessionStateId == quizGameSessionStateId 
                && x.ApplicationUserId == applicationUserId);

        if (playerScore != null)
        {
            playerScore.Score = score;
            await _context.SaveChangesAsync();
        }
    }
}
