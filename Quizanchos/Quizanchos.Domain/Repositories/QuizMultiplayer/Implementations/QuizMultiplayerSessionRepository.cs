using Microsoft.EntityFrameworkCore;
using Quizanchos.Domain.Entities.QuizMultiplayer;
using Quizanchos.Domain.Repositories.QuizMultiplayer.Interfaces;

namespace Quizanchos.Domain.Repositories.QuizMultiplayer.Implementations;

public class QuizMultiplayerSessionRepository : IQuizMultiplayerSessionRepository
{
    private readonly QuizanchosDbContext _context;

    public QuizMultiplayerSessionRepository(QuizanchosDbContext context)
    {
        _context = context;
    }

    public async Task<QuizMultiplayerSessionState?> GetByGameSessionIdAsync(Guid gameSessionId)
    {
        return await _context.QuizMultiplayerSessionStates
            .Include(x => x.GameSession)
                .ThenInclude(x => x.Players)
            .FirstOrDefaultAsync(x => x.GameSessionId == gameSessionId);
    }

    public async Task<QuizMultiplayerSessionState> CreateAsync(QuizMultiplayerSessionState state)
    {
        _context.QuizMultiplayerSessionStates.Add(state);
        await _context.SaveChangesAsync();
        return state;
    }

    public async Task UpdateAsync(QuizMultiplayerSessionState state)
    {
        _context.QuizMultiplayerSessionStates.Update(state);
        await _context.SaveChangesAsync();
    }
}
