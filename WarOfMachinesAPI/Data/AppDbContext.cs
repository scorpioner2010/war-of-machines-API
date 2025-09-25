using Microsoft.EntityFrameworkCore;
using WarOfMachines.Models;

namespace WarOfMachines.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Player> Players => Set<Player>();
        public DbSet<Vehicle> Vehicles => Set<Vehicle>();
        public DbSet<UserVehicle> UserVehicles => Set<UserVehicle>();
        public DbSet<Match> Matches => Set<Match>();
        public DbSet<MatchParticipant> MatchParticipants => Set<MatchParticipant>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ---- Vehicle ----
            modelBuilder.Entity<Vehicle>()
                .HasIndex(v => v.Code)
                .IsUnique();

            modelBuilder.Entity<Vehicle>()
                .Property(v => v.Code)
                .IsRequired();

            modelBuilder.Entity<Vehicle>()
                .Property(v => v.Name)
                .IsRequired();

            // ---- UserVehicle ----
            modelBuilder.Entity<UserVehicle>()
                .HasIndex(uv => new { uv.UserId, uv.VehicleId })
                .IsUnique();

            modelBuilder.Entity<UserVehicle>()
                .HasIndex(nameof(UserVehicle.UserId), nameof(UserVehicle.IsActive))
                .HasFilter("\"IsActive\" = TRUE")
                .IsUnique();

            modelBuilder.Entity<UserVehicle>()
                .HasOne(uv => uv.User)
                .WithMany()
                .HasForeignKey(uv => uv.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserVehicle>()
                .HasOne(uv => uv.Vehicle)
                .WithMany()
                .HasForeignKey(uv => uv.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---- MatchParticipant ----
            modelBuilder.Entity<MatchParticipant>()
                .HasOne(mp => mp.Match)
                .WithMany()
                .HasForeignKey(mp => mp.MatchId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MatchParticipant>()
                .HasOne(mp => mp.User)
                .WithMany()
                .HasForeignKey(mp => mp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MatchParticipant>()
                .HasOne(mp => mp.Vehicle)
                .WithMany()
                .HasForeignKey(mp => mp.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
