using System;
using System.Linq;
using WarOfMachines.Models;

namespace WarOfMachines.Data
{
    public static class SeedData
    {
        public static void Initialize(AppDbContext db)
        {
            // --- Factions ---
            var iron = db.Factions.FirstOrDefault(f => f.Code == "iron_alliance")
                       ?? db.Factions.Add(new Faction
                       {
                           Code = "iron_alliance",
                           Name = "Iron Alliance",
                           Description = "Veteran pilots in armored warframes forged for frontal assaults."
                       }).Entity;

            var nova = db.Factions.FirstOrDefault(f => f.Code == "nova_syndicate")
                       ?? db.Factions.Add(new Faction
                       {
                           Code = "nova_syndicate",
                           Name = "Nova Syndicate",
                           Description = "A covert network fielding agile, high-tech combat machines."
                       }).Entity;

            db.SaveChanges();

            // --- Maps ---
            if (!db.Maps.Any())
            {
                db.Maps.AddRange(
                    new Map { Code = "demo_map", Name = "Demo Yard", Description = "Training pit for rookie pilots." },
                    new Map { Code = "steel_arena", Name = "Steel Arena", Description = "Circular proving grounds with scattered cover." }
                );
                db.SaveChanges();
            }

            // --- Vehicles ---
            if (!db.Vehicles.Any())
            {
                // Simplified seeding: one basic starter for each faction
                var iaStarter = new Vehicle
                {
                    Code = "ia_l1_starter",
                    Name = "IA Skirmisher L1",
                    FactionId = iron.Id,
                    Branch = "tracked",
                    Class = VehicleClass.Scout,
                    Level = 1,
                    PurchaseCost = 0,

                    HP = 120, Damage = 12, Penetration = 40,
                    ReloadTime = 2.5f, Accuracy = 0.85f, AimTime = 1.8f,
                    Speed = 6.0f, Acceleration = 3.5f, TraverseSpeed = 35f, TurretTraverseSpeed = 30f,
                    TurretArmorFront = 40, TurretArmorSide = 25, TurretArmorRear = 20,
                    HullArmorFront = 50, HullArmorSide = 30, HullArmorRear = 25
                };

                var nvStarter = new Vehicle
                {
                    Code = "nv_l1_starter",
                    Name = "Nova Wisp L1",
                    FactionId = nova.Id,
                    Branch = "biped",
                    Class = VehicleClass.Scout,
                    Level = 1,
                    PurchaseCost = 0,

                    HP = 100, Damage = 14, Penetration = 60,
                    ReloadTime = 2.2f, Accuracy = 0.86f, AimTime = 1.6f,
                    Speed = 7.0f, Acceleration = 4.0f, TraverseSpeed = 38f, TurretTraverseSpeed = 34f,
                    TurretArmorFront = 35, TurretArmorSide = 22, TurretArmorRear = 18,
                    HullArmorFront = 40, HullArmorSide = 26, HullArmorRear = 20
                };

                db.Vehicles.AddRange(iaStarter, nvStarter);
                db.SaveChanges();
            }

            // --- Players ---
            if (!db.Players.Any())
            {
                var user = new Player
                {
                    Username = "testuser",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("test123"),
                    IsAdmin = false,
                    Mmr = 1000,
                    FreeXp = 0,
                    Bolts = 10000,
                    Adamant = 0
                };
                db.Players.Add(user);
                db.SaveChanges();

                // Starter robot assignment
                var starter = db.Vehicles.First(v => v.Code == "ia_l1_starter");
                db.UserVehicles.Add(new UserVehicle
                {
                    UserId = user.Id,
                    VehicleId = starter.Id,
                    IsActive = true,
                    Xp = 0
                });
                db.SaveChanges();
            }

            // --- Demo Match ---
            if (!db.Matches.Any())
            {
                var m = new Match
                {
                    Map = "demo_map",
                    StartedAt = DateTimeOffset.UtcNow.AddMinutes(-10),
                    EndedAt = DateTimeOffset.UtcNow.AddMinutes(-5)
                };
                db.Matches.Add(m);
                db.SaveChanges();

                var u = db.Players.First();
                var starter = db.Vehicles.First(v => v.Code == "ia_l1_starter");
                db.MatchParticipants.Add(new MatchParticipant
                {
                    MatchId = m.Id,
                    UserId = u.Id,
                    VehicleId = starter.Id,
                    Team = 1,
                    Result = "win",
                    Kills = 2,
                    Damage = 120,
                    XpEarned = 50,
                    MmrDelta = 10
                });
                db.SaveChanges();
            }
        }
    }
}
