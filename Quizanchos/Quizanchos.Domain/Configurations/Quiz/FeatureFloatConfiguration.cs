using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quizanchos.Common.Quiz.FeatureTypes;
using Quizanchos.Domain.Entities.Quiz;

namespace Quizanchos.Domain.Configurations.Quiz;

public class FeatureFloatConfiguration : IEntityTypeConfiguration<FeatureFloat>
{
    public void Configure(EntityTypeBuilder<FeatureFloat> builder)
    {
        builder.Property(f => f.Value)
            .HasConversion(featureValueFloat => featureValueFloat.Value, value => new FeatureValueFloat(value));
    }
}
