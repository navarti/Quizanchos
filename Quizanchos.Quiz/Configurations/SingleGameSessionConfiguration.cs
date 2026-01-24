using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quizanchos.Quiz.Entities;

namespace Quizanchos.Quiz.Configurations;

public class SingleGameSessionConfiguration : IEntityTypeConfiguration<SingleGameSession>
{
    public void Configure(EntityTypeBuilder<SingleGameSession> builder)
    {
        builder.HasKey(x => x.Id);
    }
}
