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
                    existing.ViewRange = v.ViewRange;
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

                HP = 105, ShellSpeed = 247.5f, ShellsCount = 87, DamageMin = 52.8f, DamageMax = 79.2f, Penetration = 88, ViewRange = 105f,
                ReloadTime = 2.2f, Accuracy = 1.4f, AimTime = 1.75f,
                Speed = 7.68f, Acceleration = 3.6f, TraverseSpeed = 34f, TurretTraverseSpeed = 26f,
                TurretArmorFront = 32, TurretArmorSide = 20, TurretArmorRear = 16,
                HullArmorFront = 30, HullArmorSide = 19, HullArmorRear = 14,
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
                HP = 180, ShellSpeed = 315f, ShellsCount = 149, DamageMin = 36f, DamageMax = 54f, Penetration = 72, ViewRange = 125f,
                ReloadTime = 1.25f, Accuracy = 0.95f, AimTime = 1.15f,
                Speed = 8.64f, Acceleration = 4.0f, TraverseSpeed = 39f, TurretTraverseSpeed = 32f,
                TurretArmorFront = 50, TurretArmorSide = 31, TurretArmorRear = 22,
                HullArmorFront = 46, HullArmorSide = 28, HullArmorRear = 20,
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
                HP = 250, ShellSpeed = 191.25f, ShellsCount = 65, DamageMin = 86.4f, DamageMax = 129.6f, Penetration = 115, ViewRange = 100f,
                ReloadTime = 3.0f, Accuracy = 1.9f, AimTime = 2.35f,
                Speed = 7.08f, Acceleration = 3.1f, TraverseSpeed = 33f, TurretTraverseSpeed = 25f,
                TurretArmorFront = 76, TurretArmorSide = 50, TurretArmorRear = 34,
                HullArmorFront = 67, HullArmorSide = 48, HullArmorRear = 32,
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
                HP = 340, ShellSpeed = 112.5f, ShellsCount = 50, DamageMin = 115.2f, DamageMax = 172.8f, Penetration = 150, ViewRange = 85f,
                ReloadTime = 4.0f, Accuracy = 3.0f, AimTime = 3.3f,
                Speed = 5.64f, Acceleration = 2.4f, TraverseSpeed = 27f, TurretTraverseSpeed = 15f,
                TurretArmorFront = 116, TurretArmorSide = 72, TurretArmorRear = 50,
                HullArmorFront = 102, HullArmorSide = 66, HullArmorRear = 46,
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

                HP = 100, ShellSpeed = 281.25f, ShellsCount = 118, DamageMin = 38.4f, DamageMax = 57.6f, Penetration = 92, ViewRange = 110f,
                ReloadTime = 1.6f, Accuracy = 1.2f, AimTime = 1.45f,
                Speed = 3.24f, Acceleration = 1.5f, TraverseSpeed = 62f, TurretTraverseSpeed = 67f,
                TurretArmorFront = 30, TurretArmorSide = 19, TurretArmorRear = 14,
                HullArmorFront = 29, HullArmorSide = 18, HullArmorRear = 13,
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
                HP = 180, ShellSpeed = 337.5f, ShellsCount = 185, DamageMin = 28.8f, DamageMax = 43.2f, Penetration = 82, ViewRange = 130f,
                ReloadTime = 1.0f, Accuracy = 0.8f, AimTime = 0.95f,
                Speed = 3.72f, Acceleration = 1.6f, TraverseSpeed = 72f, TurretTraverseSpeed = 80f,
                TurretArmorFront = 48, TurretArmorSide = 30, TurretArmorRear = 21,
                HullArmorFront = 43, HullArmorSide = 27, HullArmorRear = 19,
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
                HP = 250, ShellSpeed = 225f, ShellsCount = 75, DamageMin = 74.4f, DamageMax = 112.8f, Penetration = 122, ViewRange = 105f,
                ReloadTime = 2.6f, Accuracy = 1.65f, AimTime = 2.05f,
                Speed = 3.12f, Acceleration = 1.3f, TraverseSpeed = 66f, TurretTraverseSpeed = 71f,
                TurretArmorFront = 74, TurretArmorSide = 48, TurretArmorRear = 32,
                HullArmorFront = 67, HullArmorSide = 45, HullArmorRear = 30,
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
                HP = 335, ShellSpeed = 146.25f, ShellsCount = 57, DamageMin = 100.8f, DamageMax = 151.2f, Penetration = 145, ViewRange = 90f,
                ReloadTime = 3.5f, Accuracy = 2.65f, AimTime = 2.9f,
                Speed = 2.52f, Acceleration = 1.1f, TraverseSpeed = 60f, TurretTraverseSpeed = 61f,
                TurretArmorFront = 106, TurretArmorSide = 70, TurretArmorRear = 48,
                HullArmorFront = 94, HullArmorSide = 64, HullArmorRear = 45,
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
