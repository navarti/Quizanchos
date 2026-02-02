using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quizanchos.Domain.Entities.Quiz;

namespace Quizanchos.Domain.Configurations.Quiz;

public class QuizEntityConfiguration : IEntityTypeConfiguration<QuizEntity>
{
    public void Configure(EntityTypeBuilder<QuizEntity> builder)
    {
        builder.HasKey(x => x.Id);
    }
}
