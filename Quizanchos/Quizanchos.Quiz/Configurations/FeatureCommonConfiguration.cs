using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quizanchos.Quiz.Entities.Abstractions;

namespace Quizanchos.Quiz.Configurations;

public class FeatureCommonConfiguration : IEntityTypeConfiguration<FeatureAbstract>
{
    public void Configure(EntityTypeBuilder<FeatureAbstract> builder)
    {
        builder.HasKey(x => x.Id);
    }
}
