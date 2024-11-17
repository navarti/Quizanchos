﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quizanchos.Common.FeatureTypes;
using Quizanchos.Domain.Entities.Features;

namespace Quizanchos.Domain.Configurations;

public class FeatureIntConfiguration : IEntityTypeConfiguration<FeatureInt>
{
    public void Configure(EntityTypeBuilder<FeatureInt> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(f => f.Value)
            .HasConversion(featureValueInt => featureValueInt.Value, value => new FeatureValueInt(value));
    }
}
