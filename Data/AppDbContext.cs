using FantasyPitchXI.Models;
using Microsoft.EntityFrameworkCore;

namespace FantasyPitchXI.Data
{
    public class AppDbContext: DbContext
    {

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Player> Player { get; set; }
        public DbSet<Club> Club { get; set; }
        public DbSet<FantasyTeam> FantasyTeam { get; set; }
        public DbSet<FantasyTeamPlayer> FantasyTeamPlayer { get; set; }

        public DbSet<FantasyTeamLineup> FantasyTeamLineups { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FantasyTeamPlayer>()
                .HasKey(ftp => new
                {
                    ftp.FantasyTeamId, ftp.PlayerId
                });

            modelBuilder.Entity<FantasyTeamPlayer>()
                .HasOne(ftp => ftp.Player)
                .WithMany(p => p.FantasyTeamPlayers)
                .HasForeignKey(ftp => ftp.PlayerId);

            modelBuilder.Entity<FantasyTeamPlayer>()
                .HasOne(ftp => ftp.FantasyTeam)
                .WithMany(ft => ft.FantasyTeamPlayers)
                .HasForeignKey(ftp => ftp.FantasyTeamId);

            modelBuilder.Entity<FantasyTeam>()
                .HasIndex(t => t.TeamName).IsUnique();

            modelBuilder.Entity<FantasyTeamLineup>()
                .HasOne(ftl => ftl.FantasyTeamPlayer)
                .WithMany(ftp => ftp.FantasyTeamLineups)
                .HasForeignKey(ftl => new
                {
                    ftl.FantasyTeamId,
                    ftl.PlayerId
                });

            modelBuilder.Entity<FantasyTeamLineup>()
                .HasIndex(ftl => new {
                    ftl.FantasyTeamId,
                    ftl.PlayerId,
                    ftl.Gameweek
                }).IsUnique();

            modelBuilder.Entity<FantasyTeamLineup>().Property(ftl => ftl.IsCaptain).HasDefaultValue(false);
            modelBuilder.Entity<FantasyTeamLineup>().Property(ftl => ftl.IsViceCaptain).HasDefaultValue(false);

            modelBuilder.Entity<FantasyTeamLineup>()
                .ToTable(t => t.HasCheckConstraint("CK_Captain_Vice", "NOT ([IsCaptain] = 1 AND [IsViceCaptain] = 1)"));


            modelBuilder.Entity<Club>().HasData(
                new Club { Id = 1, Name = "Manchester City" },
                new Club { Id = 2, Name = "Arsenal" },
                new Club { Id = 3, Name = "Manchester United" },
                new Club { Id = 4, Name = "Liverpool" },
                new Club { Id = 5, Name = "Chelsea" },
                new Club { Id = 6, Name = "Bournemouth" });

            base.OnModelCreating(modelBuilder);
        }
    }
}
