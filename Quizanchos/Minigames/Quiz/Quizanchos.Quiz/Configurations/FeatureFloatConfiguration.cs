using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quizanchos.Common.FeatureTypes;
using Quizanchos.Quiz.Entities;

namespace Quizanchos.Quiz.Configurations;

public class FeatureFloatConfiguration : IEntityTypeConfiguration<FeatureFloat>
{
    public void Configure(EntityTypeBuilder<FeatureFloat> builder)
    {
        builder.Property(f => f.Value)
            .HasConversion(featureValueFloat => featureValueFloat.Value, value => new FeatureValueFloat(value));
    }
}
