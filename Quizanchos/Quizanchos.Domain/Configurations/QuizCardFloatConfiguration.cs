using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quizanchos.Domain.Entities;

namespace Quizanchos.Domain.Configurations;

internal class QuizCardFloatConfiguration : IEntityTypeConfiguration<QuizCardFloat>
{
    public void Configure(EntityTypeBuilder<QuizCardFloat> builder)
    {
        builder.HasOne(x => x.Option1)
            .WithMany()
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Option2)
            .WithMany()
            .OnDelete(DeleteBehavior.NoAction);
    }
}
