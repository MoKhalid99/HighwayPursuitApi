using HighwayPursuitApi.Models;
using Microsoft.EntityFrameworkCore;

namespace HighwayPursuitApi.Data;

public class GameDbContext : DbContext
{
	public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }

	public DbSet<Player> Players => Set<Player>();
	public DbSet<Score> Scores => Set<Score>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		// for user name uniqueness
		modelBuilder.Entity<Player>()
			.HasIndex(p => p.Username)
			.IsUnique();

		// Add Top Scores
		modelBuilder.Entity<Score>()
			.HasIndex(s => s.ScoreValue);
	}
}