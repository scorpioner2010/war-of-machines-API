using System;
using System.Linq;
using System.Text.Json;
using WarOfMachines.Models;

namespace WarOfMachines.Data
{
    public static class SeedData
    {
        public static void Initialize(AppDbContext db)
        {
            // --- Players (1 запис) ---
            if (!db.Players.Any())
            {
                var user = new Player
                {
                    Username = "testuser",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("test123"),
                    IsAdmin = false,
                    XpTotal = 50,
                    Mmr = 1000
                };
                db.Players.Add(user);
                db.SaveChanges();
            }
            var firstUser = db.Players.First();

            // --- Vehicles (3 типи: starter, heavy, scout) ---
            if (!db.Vehicles.Any())
            {
                db.Vehicles.AddRange(
                    new Vehicle
                    {
                        Code = "starter",
                        Name = "Starter Bot",
                        Stats = JsonDocument.Parse("""{ "hp": 100, "dmg": 10, "speed": 5 }""")
                    },
                    new Vehicle
                    {
                        Code = "heavy",
                        Name = "Heavy Tank Bot",
                        Stats = JsonDocument.Parse("""{ "hp": 300, "dmg": 30, "speed": 2 }""")
                    },
                    new Vehicle
                    {
                        Code = "scout",
                        Name = "Scout Drone",
                        Stats = JsonDocument.Parse("""{ "hp": 70, "dmg": 7, "speed": 8 }""")
                    }
                );
                db.SaveChanges();
            }

            var starter = db.Vehicles.First(v => v.Code == "starter");
            var heavy   = db.Vehicles.First(v => v.Code == "heavy");
            var scout   = db.Vehicles.First(v => v.Code == "scout");

            // --- UserVehicles (видати користувачу тільки starter як активний) ---
            if (!db.UserVehicles.Any())
            {
                db.UserVehicles.Add(new UserVehicle
                {
                    UserId = firstUser.Id,
                    VehicleId = starter.Id,
                    IsActive = true
                });
                db.SaveChanges();
            }

            // --- Matches (1 запис) ---
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
            }
            var firstMatch = db.Matches.First();

            // --- MatchParticipants (1 запис для testuser зі starter) ---
            if (!db.MatchParticipants.Any())
            {
                db.MatchParticipants.Add(new MatchParticipant
                {
                    MatchId = firstMatch.Id,
                    UserId = firstUser.Id,
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
