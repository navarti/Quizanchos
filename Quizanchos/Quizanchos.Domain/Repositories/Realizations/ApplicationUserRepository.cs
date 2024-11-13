using Quizanchos.Domain.Repositories.Interfaces;

namespace Quizanchos.Domain.Repositories.Realizations;

public class ApplicationUserRepository : IApplicationUserRepository
{
    public ApplicationUserRepository(QuizDbContext dbContext)
    {
    }
}
