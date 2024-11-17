using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quizanchos.Domain.Entities;

namespace Quizanchos.Domain.Configurations;

internal class QuizCardIntConfiguration : IEntityTypeConfiguration<QuizCardInt>
{
    public void Configure(EntityTypeBuilder<QuizCardInt> builder)
    {
        builder.HasOne(x => x.Option1)
            .WithMany()
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Option2)
            .WithMany()
            .OnDelete(DeleteBehavior.NoAction);
    }
}
