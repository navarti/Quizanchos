using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quizanchos.Domain.Entities.Quiz.Abstractions;

namespace Quizanchos.Domain.Configurations.Quiz;

public class FeatureCommonConfiguration : IEntityTypeConfiguration<FeatureAbstract>
{
    public void Configure(EntityTypeBuilder<FeatureAbstract> builder)
    {
        builder.HasKey(x => x.Id);
    }
}
