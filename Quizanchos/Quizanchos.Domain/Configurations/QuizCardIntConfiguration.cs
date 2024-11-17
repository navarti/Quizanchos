using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quizanchos.Domain.Entities;

namespace Quizanchos.Domain.Configurations;

internal class QuizCardIntConfiguration
{
    public void Configure(EntityTypeBuilder<QuizCardInt> builder)
    {
        // TODO: make unique constraint for SingleGameSession and CardIndex
        builder.HasKey(x => x.Id);
    }
}
