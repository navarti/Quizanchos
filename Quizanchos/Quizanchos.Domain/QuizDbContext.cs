﻿using Microsoft.EntityFrameworkCore;
using Quizanchos.Domain.Configurations;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Entities.Features;

namespace Quizanchos.Domain;

public class QuizDbContext : DbContext
{
    public QuizDbContext(DbContextOptions<QuizDbContext> options) : base(options)
    {
    }

    public DbSet<QuizEntity> QuizEntities { get; set; }
    public DbSet<QuizCategory> QuizCategories { get; set; }
    public DbSet<FeatureInt> FeatureInts { get; set; }
    public DbSet<FeatureFloat> FeatureFloats { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new QuizEntityConfiguration());
        modelBuilder.ApplyConfiguration(new QuizCategoryConfiguration());
        modelBuilder.ApplyConfiguration(new FeatureConfiguration());
        modelBuilder.ApplyConfiguration(new FeatureIntConfiguration());
        modelBuilder.ApplyConfiguration(new FeatureFloatConfiguration());
    }
}
