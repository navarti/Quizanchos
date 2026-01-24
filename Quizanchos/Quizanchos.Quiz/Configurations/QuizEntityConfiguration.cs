using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quizanchos.Quiz.Entities;

namespace Quizanchos.Quiz.Configurations;

public class QuizEntityConfiguration : IEntityTypeConfiguration<QuizEntity>
{
    public void Configure(EntityTypeBuilder<QuizEntity> builder)
    {
        builder.HasKey(x => x.Id);
    }
}
