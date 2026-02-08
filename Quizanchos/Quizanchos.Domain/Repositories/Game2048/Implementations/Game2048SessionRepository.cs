using Microsoft.EntityFrameworkCore;
using Quizanchos.Domain.Entities.Game2048;
using Quizanchos.Domain.Repositories.Game2048.Interfaces;

namespace Quizanchos.Domain.Repositories.Game2048.Implementations;

public class Game2048SessionRepository : IGame2048SessionRepository
{
    private readonly QuizanchosDbContext _context;

    public Game2048SessionRepository(QuizanchosDbContext context)
    {
        _context = context;
    }

    public async Task<Game2048SessionState?> GetByGameSessionIdAsync(Guid gameSessionId)
    {
        return await _context.Game2048SessionStates
            .Include(x => x.GameSession)
                .ThenInclude(x => x.Players)
            .FirstOrDefaultAsync(x => x.GameSessionId == gameSessionId);
    }

    public async Task<Game2048SessionState> CreateAsync(Game2048SessionState state)
    {
        _context.Game2048SessionStates.Add(state);
        await _context.SaveChangesAsync();
        return state;
    }

    public async Task UpdateAsync(Game2048SessionState state)
    {
        _context.Game2048SessionStates.Update(state);
        await _context.SaveChangesAsync();
    }
}
