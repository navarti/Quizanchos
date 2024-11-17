using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quizanchos.Domain.Entities;

namespace Quizanchos.Domain.Configurations;

internal class QuizCardFloatConfiguration
{
    public void Configure(EntityTypeBuilder<QuizCardFloat> builder)
    {
        // TODO: make unique constraint for SingleGameSession and CardIndex
        builder.HasKey(x => x.Id);
    }
}
