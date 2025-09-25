using System.Linq;
using System.Text.Json;
using WarOfMachines.Models;

namespace WarOfMachines.Data
{
    public static class SeedData
    {
        public static void Initialize(AppDbContext db)
        {
            // --- Factions ---
            var iron = db.Factions.FirstOrDefault(f => f.Code == "iron_alliance")
                       ?? db.Factions.Add(new Faction { Code = "iron_alliance", Name = "Залізний Альянс", Description = "Фракція важких мехів і сталі" }).Entity;

            var nova = db.Factions.FirstOrDefault(f => f.Code == "nova_syndicate")
                       ?? db.Factions.Add(new Faction { Code = "nova_syndicate", Name = "Нова Синдикат", Description = "Фракція високих технологій і мобільності" }).Entity;

            db.SaveChanges();

            // --- Maps ---
            if (!db.Maps.Any())
            {
                db.Maps.AddRange(
                    new Map { Code = "demo_map", Name = "Demo Yard", Description = "Невелика тестова арена" },
                    new Map { Code = "steel_arena", Name = "Steel Arena", Description = "Кільцева арена з укриттями" }
                );
                db.SaveChanges();
            }

            // --- Vehicles (2 фракції * 3 роботи) ---
            if (!db.Vehicles.Any())
            {
                db.Vehicles.AddRange(
                    new Vehicle { Code = "starter",  Name = "IA Scout Mk.I", FactionId = iron.Id, Branch = "tracked", Stats = JsonDocument.Parse("""{ "hp": 120, "dmg": 12, "speed": 6 }""") },
                    new Vehicle { Code = "ia_heavy", Name = "IA Heavy Mk.II", FactionId = iron.Id, Branch = "tracked", Stats = JsonDocument.Parse("""{ "hp": 320, "dmg": 30, "speed": 2 }""") },
                    new Vehicle { Code = "ia_biped", Name = "IA Strider",    FactionId = iron.Id, Branch = "biped",   Stats = JsonDocument.Parse("""{ "hp": 180, "dmg": 18, "speed": 5 }""") },

                    new Vehicle { Code = "nv_light",   Name = "Nova Swift",   FactionId = nova.Id, Branch = "biped",   Stats = JsonDocument.Parse("""{ "hp": 100, "dmg": 14, "speed": 7 }""") },
                    new Vehicle { Code = "nv_tank",    Name = "Nova Bulwark", FactionId = nova.Id, Branch = "tracked", Stats = JsonDocument.Parse("""{ "hp": 280, "dmg": 26, "speed": 3 }""") },
                    new Vehicle { Code = "nv_assault", Name = "Nova Raptor",  FactionId = nova.Id, Branch = "biped",   Stats = JsonDocument.Parse("""{ "hp": 160, "dmg": 22, "speed": 6 }""") }
                );
                db.SaveChanges();
            }

            // --- Players (1 запис) + стартовий юніт ---
            if (!db.Players.Any())
            {
                var user = new Player
                {
                    Username = "testuser",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("test123"),
                    IsAdmin = false,
                    XpTotal = 50,
                    Mmr = 1000,
                    Bolts = 10000,
                    Adamant = 0
                };
                db.Players.Add(user);
                db.SaveChanges();

                var starter = db.Vehicles.First(v => v.Code == "starter");
                db.UserVehicles.Add(new UserVehicle { UserId = user.Id, VehicleId = starter.Id, IsActive = true });
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
                var starter = db.Vehicles.First(v => v.Code == "starter");
                db.MatchParticipants.Add(new MatchParticipant
                {
                    MatchId = m.Id, UserId = u.Id, VehicleId = starter.Id,
                    Team = 1, Result = "win", Kills = 2, Damage = 120, XpEarned = 50, MmrDelta = 10
                });
                db.SaveChanges();
            }
        }
    }
}
