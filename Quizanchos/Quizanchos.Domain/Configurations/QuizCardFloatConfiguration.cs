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
            .HasForeignKey(x => x.Option1Id);

        builder.HasOne(x => x.Option2)
            .WithMany()
            .HasForeignKey(x => x.Option2Id);
    }
}
