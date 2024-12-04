using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Entities.Abstractions;
using System.Text.Json;

namespace Quizanchos.Domain.Configurations;

internal class QuizCardIntConfiguration : IEntityTypeConfiguration<QuizCardInt>
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
