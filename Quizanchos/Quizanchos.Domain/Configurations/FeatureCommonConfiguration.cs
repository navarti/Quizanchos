using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quizanchos.Domain.Entities.Abstractions;

namespace Quizanchos.Domain.Configurations;

internal class FeatureCommonConfiguration : IEntityTypeConfiguration<FeatureAbstract>
{
    public void Configure(EntityTypeBuilder<FeatureAbstract> builder)
    {
        builder.HasKey(x => x.Id);
    }
}
