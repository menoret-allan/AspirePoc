using Microsoft.EntityFrameworkCore;

namespace TestAspire.ApiService.Entities;

public class MyDbContext : DbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }

    public DbSet<Algo> Algos { get; set; }
    public DbSet<Dataset> Datasets { get; set; }
    public DbSet<Result> Results { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Dataset>()
            .HasMany(e => e.Results)
            .WithOne(e => e.Dataset)
            .HasForeignKey(e => e.DatasetId)
            .HasPrincipalKey(e => e.Id);
        modelBuilder.Entity<Algo>()
            .HasMany(e => e.Results)
            .WithOne(e => e.Algo)
            .HasForeignKey(e => e.AlgoId)
            .HasPrincipalKey(e => e.Id);
    }
}