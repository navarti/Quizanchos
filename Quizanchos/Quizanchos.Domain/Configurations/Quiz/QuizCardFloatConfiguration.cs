using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quizanchos.Domain.Entities.Quiz;
using Quizanchos.Domain.Entities.Quiz.Abstractions;
using System.Text.Json;

namespace Quizanchos.Domain.Configurations.Quiz;

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
            .Metadata.SetValueComparer(
                new ValueComparer<List<FeatureFloat>>(
                    (c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));

        builder.Property(q => q.Options)
            .HasColumnType("text");
    }
}
