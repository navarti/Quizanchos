﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Quizanchos.Domain.Configurations;
using Quizanchos.Domain.Entities;

namespace Quizanchos.Domain;

public class QuizDbContext : IdentityDbContext<ApplicationUser>
{
    public QuizDbContext(DbContextOptions<QuizDbContext> options) : base(options)
    {
    }

    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<QuizEntity> QuizEntities { get; set; }
    public DbSet<QuizCategory> QuizCategories { get; set; }
    public DbSet<FeatureInt> FeatureInts { get; set; }
    public DbSet<FeatureFloat> FeatureFloats { get; set; }
    public DbSet<SingleGameSession> SingleGameSessions { get; set; }
    public DbSet<QuizCardFloat> QuizCardFloats { get; set; }
    public DbSet<QuizCardInt> QuizCardInts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new ApplicationUserConfiguration());
        modelBuilder.ApplyConfiguration(new QuizEntityConfiguration());
        modelBuilder.ApplyConfiguration(new QuizCategoryConfiguration());
        modelBuilder.ApplyConfiguration(new FeatureCommonConfiguration());
        modelBuilder.ApplyConfiguration(new FeatureIntConfiguration());
        modelBuilder.ApplyConfiguration(new FeatureFloatConfiguration());
        modelBuilder.ApplyConfiguration(new SingleGameSessionConfiguration());
        modelBuilder.ApplyConfiguration(new QuizCardCommonConfiguration());
        modelBuilder.ApplyConfiguration(new QuizCardFloatConfiguration());
        modelBuilder.ApplyConfiguration(new QuizCardIntConfiguration());
    }
}
