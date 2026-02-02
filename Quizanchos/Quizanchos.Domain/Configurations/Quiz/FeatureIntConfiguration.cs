using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quizanchos.Common.Quiz.FeatureTypes;
using Quizanchos.Domain.Entities.Quiz;

namespace Quizanchos.Domain.Configurations.Quiz;

public class FeatureIntConfiguration : IEntityTypeConfiguration<FeatureInt>
{
    public void Configure(EntityTypeBuilder<FeatureInt> builder)
    {
        builder.Property(f => f.Value)
            .HasConversion(featureValueInt => featureValueInt.Value, value => new FeatureValueInt(value));
    }
}
