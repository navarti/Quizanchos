﻿using Microsoft.EntityFrameworkCore;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;

namespace Quizanchos.Domain.Repositories.Realizations;

public class QuizCategoryRepository : EntityRepositoryBase<Guid, QuizCategory>, IQuizCategoryRepository
{
    public QuizCategoryRepository(QuizDbContext dbContext) : base(dbContext)
    {
    }

    public Task<QuizCategory> GetByName(string name)
    {
        return dbSet.FirstOrDefaultAsync(quizCategory => quizCategory.Name == name);
    }
}
