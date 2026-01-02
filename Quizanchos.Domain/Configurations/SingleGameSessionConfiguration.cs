using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quizanchos.Domain.Entities;

namespace Quizanchos.Domain.Configurations;

public class SingleGameSessionConfiguration : IEntityTypeConfiguration<SingleGameSession>
{
    public void Configure(EntityTypeBuilder<SingleGameSession> builder)
    {
        builder.HasKey(x => x.Id);
    }
}
