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
            .Metadata.SetValueComparer(
                new ValueComparer<List<FeatureInt>>(
                    (c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));

        builder.Property(q => q.Options)
            .HasColumnType("nvarchar(max)");
    }
}
