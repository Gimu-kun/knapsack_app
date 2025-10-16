using knapsack_app.Models;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<UserModel> User { get; set; }
    public DbSet<AdminModel> Admin { get; set; }
    public DbSet<KsChallengeModel> KsChallenge { get; set; }
    public DbSet<TakenModel> Taken{ get; set; }
    public DbSet<UserSessionLogModel> UserSessionLog{ get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<KsChallengeModel>()
        .Property(e => e.Difficulty)
        .HasConversion<string>()
        .HasMaxLength(20);
    }

}