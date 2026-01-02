using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quizanchos.Domain.Entities.Abstractions;

namespace Quizanchos.Domain.Configurations;

internal class QuizCardCommonConfiguration : IEntityTypeConfiguration<QuizCardAbstract>
{
    public void Configure(EntityTypeBuilder<QuizCardAbstract> builder)
    {
        // TODO: make unique constraint for SingleGameSession and CardIndex
        builder.HasKey(x => x.Id);

        builder.HasOne(q => q.SingleGameSession)
            .WithMany()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
