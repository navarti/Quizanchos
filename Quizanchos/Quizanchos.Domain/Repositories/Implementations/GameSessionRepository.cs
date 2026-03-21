using Microsoft.EntityFrameworkCore;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;

namespace Quizanchos.Domain.Repositories.Implementations;

public class GameSessionRepository : IGameSessionRepository
{
    private readonly QuizanchosDbContext _context;

    public GameSessionRepository(QuizanchosDbContext context)
    {
        _context = context;
    }

    public async Task<GameSession?> GetByIdAsync(Guid id)
    {
        return await _context.GameSessions
            .Include(x => x.Players)
            .Include(x => x.Winner)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<GameSession?> GetActiveByPlayerIdAsync(string playerId)
    {
       return await _context.GameSessions
            .Include(x => x.Players)
            .Include(x => x.Winner)
            .FirstOrDefaultAsync(x => x.IsActive && x.Players.Any(p => p.ApplicationUserId == playerId));
    }

    public async Task<GameSession> CreateAsync(GameSession gameSession)
    {
        _context.GameSessions.Add(gameSession);
        await _context.SaveChangesAsync();
        return gameSession;
    }

    public async Task UpdateAsync(GameSession gameSession)
    {
        _context.GameSessions.Update(gameSession);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        GameSession? gameSession = await _context.GameSessions.FindAsync(id);
        if (gameSession == null)
            return false;

        _context.GameSessions.Remove(gameSession);
        await _context.SaveChangesAsync();
        return true;
    }
}
