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
        public DbSet<Faction> Factions => Set<Faction>();
        public DbSet<Map> Maps => Set<Map>();

        public DbSet<VehicleResearchRequirement> VehicleResearchRequirements => Set<VehicleResearchRequirement>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Faction
            modelBuilder.Entity<Faction>()
                .HasIndex(f => f.Code)
                .IsUnique();

            modelBuilder.Entity<Faction>()
                .Property(f => f.Code).IsRequired();
            modelBuilder.Entity<Faction>()
                .Property(f => f.Name).IsRequired();

            // Vehicle
            modelBuilder.Entity<Vehicle>()
                .HasIndex(v => v.Code).IsUnique();
            modelBuilder.Entity<Vehicle>()
                .Property(v => v.Code).IsRequired();
            modelBuilder.Entity<Vehicle>()
                .Property(v => v.Name).IsRequired();

            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Faction)
                .WithMany()
                .HasForeignKey(v => v.FactionId)
                .OnDelete(DeleteBehavior.Restrict);

            // VehicleResearchRequirement (предок -> нащадок; ResearchFrom прив'язане до Successor)
            modelBuilder.Entity<Vehicle>()
                .HasMany(v => v.ResearchFrom)
                .WithOne(r => r.Successor)
                .HasForeignKey(r => r.SuccessorVehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VehicleResearchRequirement>()
                .HasOne(r => r.Predecessor)
                .WithMany()
                .HasForeignKey(r => r.PredecessorVehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VehicleResearchRequirement>()
                .HasIndex(r => new { r.PredecessorVehicleId, r.SuccessorVehicleId })
                .IsUnique();

            // UserVehicle
            modelBuilder.Entity<UserVehicle>()
                .HasIndex(uv => new { uv.UserId, uv.VehicleId }).IsUnique();

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

            // MatchParticipant
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

            // Map
            modelBuilder.Entity<Map>()
                .HasIndex(m => m.Code)
                .IsUnique();

            modelBuilder.Entity<Map>()
                .Property(m => m.Code).IsRequired();
            modelBuilder.Entity<Map>()
                .Property(m => m.Name).IsRequired();
        }
    }
}
