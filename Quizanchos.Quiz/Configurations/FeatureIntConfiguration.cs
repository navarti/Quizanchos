using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quizanchos.Common.FeatureTypes;
using Quizanchos.Quiz.Entities;

namespace Quizanchos.Quiz.Configurations;

public class FeatureIntConfiguration : IEntityTypeConfiguration<FeatureInt>
{
    public void Configure(EntityTypeBuilder<FeatureInt> builder)
    {
        builder.Property(f => f.Value)
            .HasConversion(featureValueInt => featureValueInt.Value, value => new FeatureValueInt(value));
    }
}
