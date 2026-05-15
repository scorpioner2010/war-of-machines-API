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

            // ------- Локальні хелпери (ідемпотентні) -------
            Vehicle EnsureVehicle(Vehicle v)
            {
                var existing = db.Vehicles.FirstOrDefault(x => x.Code == v.Code);
                if (existing != null)
                {
                    existing.Name = v.Name;
                    existing.FactionId = v.FactionId;
                    existing.Branch = v.Branch;
                    existing.Class = v.Class;
                    existing.Level = v.Level;
                    existing.PurchaseCost = v.PurchaseCost;
                    existing.HP = v.HP;
                    existing.ShellSpeed = v.ShellSpeed;
                    existing.ShellsCount = v.ShellsCount;
                    existing.DamageMin = v.DamageMin;
                    existing.DamageMax = v.DamageMax;
                    existing.Penetration = v.Penetration;
                    existing.ReloadTime = v.ReloadTime;
                    existing.Accuracy = v.Accuracy;
                    existing.AimTime = v.AimTime;
                    existing.Speed = v.Speed;
                    existing.Acceleration = v.Acceleration;
                    existing.TraverseSpeed = v.TraverseSpeed;
                    existing.TurretTraverseSpeed = v.TurretTraverseSpeed;
                    existing.TurretArmorFront = v.TurretArmorFront;
                    existing.TurretArmorSide = v.TurretArmorSide;
                    existing.TurretArmorRear = v.TurretArmorRear;
                    existing.HullArmorFront = v.HullArmorFront;
                    existing.HullArmorSide = v.HullArmorSide;
                    existing.HullArmorRear = v.HullArmorRear;
                    existing.IsVisible = v.IsVisible;
                    db.SaveChanges();
                    return existing;
                }

                db.Vehicles.Add(v);
                db.SaveChanges();
                return v;
            }

            void EnsureLink(int predecessorId, int successorId, int requiredXp)
            {
                bool exists = db.VehicleResearchRequirements
                    .Any(r => r.PredecessorVehicleId == predecessorId && r.SuccessorVehicleId == successorId);
                if (exists)
                {
                    return;
                }

                db.VehicleResearchRequirements.Add(new VehicleResearchRequirement
                {
                    PredecessorVehicleId = predecessorId,
                    SuccessorVehicleId = successorId,
                    RequiredXpOnPredecessor = requiredXp
                });
                db.SaveChanges();
            }

            // --- Vehicles (ідемпотентно; без if (!db.Vehicles.Any())) ---

            // Iron Alliance (tracked) — L1 + три L2
            var iaStarter = EnsureVehicle(new Vehicle
            {
                Code = "ia_l1_starter",
                Name = "IA Skirmisher",
                FactionId = iron.Id,
                Branch = "tracked",
                Class = VehicleClass.Scout,
                Level = 1,
                PurchaseCost = 0,

                HP = 105, ShellSpeed = 95f, ShellsCount = 32, DamageMin = 40f, DamageMax = 58f, Penetration = 84,
                ReloadTime = 2.7f, Accuracy = 2.35f, AimTime = 1.85f,
                Speed = 6.4f, Acceleration = 3.6f, TraverseSpeed = 34f, TurretTraverseSpeed = 26f,
                TurretArmorFront = 32, TurretArmorSide = 20, TurretArmorRear = 16,
                HullArmorFront = 38, HullArmorSide = 24, HullArmorRear = 18,
                IsVisible = true
            });

            var iaL2Scout = EnsureVehicle(new Vehicle
            {
                Code = "ia_l2_scout",
                Name = "IA Strider",
                FactionId = iron.Id,
                Branch = "tracked",
                Class = VehicleClass.Scout,
                Level = 2,
                PurchaseCost = 5000,
                HP = 180, ShellSpeed = 108f, ShellsCount = 40, DamageMin = 45f, DamageMax = 65f, Penetration = 74,
                ReloadTime = 2.35f, Accuracy = 1.80f, AimTime = 1.55f,
                Speed = 7.2f, Acceleration = 4.0f, TraverseSpeed = 39f, TurretTraverseSpeed = 32f,
                TurretArmorFront = 50, TurretArmorSide = 31, TurretArmorRear = 22,
                HullArmorFront = 58, HullArmorSide = 35, HullArmorRear = 25,
                IsVisible = true
            });

            var iaL2Guardian = EnsureVehicle(new Vehicle
            {
                Code = "ia_l2_guardian",
                Name = "IA Bulwark",
                FactionId = iron.Id,
                Branch = "tracked",
                Class = VehicleClass.Guardian,
                Level = 2,
                PurchaseCost = 9000,
                HP = 250, ShellSpeed = 74f, ShellsCount = 30, DamageMin = 80f, DamageMax = 120f, Penetration = 86,
                ReloadTime = 2.85f, Accuracy = 3.35f, AimTime = 1.9f,
                Speed = 5.9f, Acceleration = 3.1f, TraverseSpeed = 33f, TurretTraverseSpeed = 25f,
                TurretArmorFront = 76, TurretArmorSide = 50, TurretArmorRear = 34,
                HullArmorFront = 84, HullArmorSide = 60, HullArmorRear = 40,
                IsVisible = true
            });

            var iaL2Colossus = EnsureVehicle(new Vehicle
            {
                Code = "ia_l2_colossus",
                Name = "IA Juggernaut",
                FactionId = iron.Id,
                Branch = "tracked",
                Class = VehicleClass.Colossus,
                Level = 2,
                PurchaseCost = 15000,
                HP = 340, ShellSpeed = 30f, ShellsCount = 18, DamageMin = 140f, DamageMax = 210f, Penetration = 96,
                ReloadTime = 3.45f, Accuracy = 5.00f, AimTime = 2.25f,
                Speed = 4.7f, Acceleration = 2.4f, TraverseSpeed = 27f, TurretTraverseSpeed = 15f,
                TurretArmorFront = 116, TurretArmorSide = 72, TurretArmorRear = 50,
                HullArmorFront = 128, HullArmorSide = 82, HullArmorRear = 58,
                IsVisible = true
            });

            // Links: L1 -> (Scout|Guardian|Colossus)
            EnsureLink(iaStarter.Id, iaL2Scout.Id,    requiredXp: 400);
            EnsureLink(iaStarter.Id, iaL2Guardian.Id, requiredXp: 700);
            EnsureLink(iaStarter.Id, iaL2Colossus.Id, requiredXp: 1000);

            // Nova Syndicate (biped) — L1 + три L2
            var nvStarter = EnsureVehicle(new Vehicle
            {
                Code = "nv_l1_starter",
                Name = "Nova Wisp",
                FactionId = nova.Id,
                Branch = "biped",
                Class = VehicleClass.Scout,
                Level = 1,
                PurchaseCost = 0,

                HP = 100, ShellSpeed = 112f, ShellsCount = 30, DamageMin = 42f, DamageMax = 60f, Penetration = 90,
                ReloadTime = 2.8f, Accuracy = 2.10f, AimTime = 1.65f,
                Speed = 2.7f, Acceleration = 1.5f, TraverseSpeed = 62f, TurretTraverseSpeed = 67f,
                TurretArmorFront = 30, TurretArmorSide = 19, TurretArmorRear = 14,
                HullArmorFront = 36, HullArmorSide = 23, HullArmorRear = 16,
                IsVisible = true
            });

            var nvL2Scout = EnsureVehicle(new Vehicle
            {
                Code = "nv_l2_scout",
                Name = "Nova Flicker",
                FactionId = nova.Id,
                Branch = "biped",
                Class = VehicleClass.Scout,
                Level = 2,
                PurchaseCost = 5000,
                HP = 180, ShellSpeed = 130f, ShellsCount = 36, DamageMin = 50f, DamageMax = 70f, Penetration = 88,
                ReloadTime = 2.7f, Accuracy = 1.50f, AimTime = 1.4f,
                Speed = 3.1f, Acceleration = 1.6f, TraverseSpeed = 72f, TurretTraverseSpeed = 80f,
                TurretArmorFront = 48, TurretArmorSide = 30, TurretArmorRear = 21,
                HullArmorFront = 54, HullArmorSide = 34, HullArmorRear = 24,
                IsVisible = true
            });

            var nvL2Guardian = EnsureVehicle(new Vehicle
            {
                Code = "nv_l2_guardian",
                Name = "Nova Aegis",
                FactionId = nova.Id,
                Branch = "biped",
                Class = VehicleClass.Guardian,
                Level = 2,
                PurchaseCost = 9000,
                HP = 250, ShellSpeed = 90f, ShellsCount = 26, DamageMin = 88f, DamageMax = 125f, Penetration = 100,
                ReloadTime = 3.15f, Accuracy = 3.00f, AimTime = 1.75f,
                Speed = 2.6f, Acceleration = 1.3f, TraverseSpeed = 66f, TurretTraverseSpeed = 71f,
                TurretArmorFront = 74, TurretArmorSide = 48, TurretArmorRear = 32,
                HullArmorFront = 84, HullArmorSide = 56, HullArmorRear = 38,
                IsVisible = true
            });

            var nvL2Colossus = EnsureVehicle(new Vehicle
            {
                Code = "nv_l2_colossus",
                Name = "Nova Titan",
                FactionId = nova.Id,
                Branch = "biped",
                Class = VehicleClass.Colossus,
                Level = 2,
                PurchaseCost = 15000,
                HP = 335, ShellSpeed = 46f, ShellsCount = 16, DamageMin = 150f, DamageMax = 220f, Penetration = 118,
                ReloadTime = 3.9f, Accuracy = 4.50f, AimTime = 2.1f,
                Speed = 2.1f, Acceleration = 1.1f, TraverseSpeed = 60f, TurretTraverseSpeed = 61f,
                TurretArmorFront = 106, TurretArmorSide = 70, TurretArmorRear = 48,
                HullArmorFront = 118, HullArmorSide = 80, HullArmorRear = 56,
                IsVisible = true
            });

            // Links: L1 -> (Scout|Guardian|Colossus)
            EnsureLink(nvStarter.Id, nvL2Scout.Id,    requiredXp: 400);
            EnsureLink(nvStarter.Id, nvL2Guardian.Id, requiredXp: 700);
            EnsureLink(nvStarter.Id, nvL2Colossus.Id, requiredXp: 1000);

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

                if (!db.UserVehicleResearches.Any(x => x.UserId == user.Id && x.VehicleId == starter.Id))
                {
                    db.UserVehicleResearches.Add(new UserVehicleResearch
                    {
                        UserId = user.Id,
                        VehicleId = starter.Id,
                        ResearchedAt = DateTimeOffset.UtcNow
                    });
                    db.SaveChanges();
                }
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
