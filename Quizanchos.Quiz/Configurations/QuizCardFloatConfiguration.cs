using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quizanchos.Quiz.Entities;
using Quizanchos.Quiz.Entities.Abstractions;
using System.Text.Json;

namespace Quizanchos.Quiz.Configurations;

public class QuizCardFloatConfiguration : IEntityTypeConfiguration<QuizCardFloat>
{
    public void Configure(EntityTypeBuilder<QuizCardFloat> builder)
    {
        builder.HasBaseType<QuizCardAbstract>();

        builder.Property(q => q.Options)
            .HasConversion(
                options => JsonSerializer.Serialize(options, (JsonSerializerOptions)null),
                json => JsonSerializer.Deserialize<List<FeatureFloat>>(json, (JsonSerializerOptions)null)
            )
            .HasColumnType("nvarchar(max)");
    }
}
