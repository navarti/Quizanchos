using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quizanchos.Domain.Entities.Quiz;
using Quizanchos.Domain.Entities.Quiz.Abstractions;
using System.Text.Json;

namespace Quizanchos.Domain.Configurations.Quiz;

public class QuizCardIntConfiguration : IEntityTypeConfiguration<QuizCardInt>
{
    public void Configure(EntityTypeBuilder<QuizCardInt> builder)
    {
        builder.HasBaseType<QuizCardAbstract>();

        builder.Property(q => q.Options)
            .HasConversion(
                options => JsonSerializer.Serialize(options, (JsonSerializerOptions)null),
                json => JsonSerializer.Deserialize<List<FeatureInt>>(json, (JsonSerializerOptions)null)
            )
            .HasColumnType("nvarchar(max)");
    }
}
