using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quizanchos.Common.FeatureTypes;
using Quizanchos.Domain.Entities.Features;

namespace Quizanchos.Domain.Configurations;

public class FeatureFloatConfiguration : IEntityTypeConfiguration<FeatureFloat>
{
    public void Configure(EntityTypeBuilder<FeatureFloat> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(f => f.Value)
            .HasConversion(featureValueFloat => featureValueFloat.Value, value => new FeatureValueFloat(value));
    }
}
