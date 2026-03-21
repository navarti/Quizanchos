using Microsoft.EntityFrameworkCore;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;

namespace Quizanchos.Domain.Repositories.Implementations;

public class GameSessionStateRepository : IGameSessionStateRepository
{
    private readonly QuizanchosDbContext _context;

    public GameSessionStateRepository(QuizanchosDbContext context)
    {
        _context = context;
    }

    public async Task<GameSessionState?> GetByGameSessionIdAsync(Guid gameSessionId)
    {
        return await _context.GameSessionStates
            .Include(x => x.GameSession)
                .ThenInclude(x => x.Players)
            .FirstOrDefaultAsync(x => x.GameSessionId == gameSessionId);
    }

    public async Task<GameSessionState> CreateAsync(GameSessionState state)
    {
        _context.GameSessionStates.Add(state);
        await _context.SaveChangesAsync();
        return state;
    }

    public async Task UpdateAsync(GameSessionState state)
    {
        _context.GameSessionStates.Update(state);
        await _context.SaveChangesAsync();
    }
}
