using System;
using System.Linq;
using WarOfMachines.Models;

namespace WarOfMachines.Data
{
    public static class SeedData
    {
        public static void Initialize(AppDbContext db)
        {
            // Factions
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

            // Maps
            if (!db.Maps.Any())
            {
                db.Maps.AddRange(
                    new Map { Code = "demo_map", Name = "Demo Yard", Description = "Training pit for rookie pilots." },
                    new Map { Code = "steel_arena", Name = "Steel Arena", Description = "Circular proving grounds with scattered cover." }
                );
                db.SaveChanges();
            }

            // Vehicles
            if (!db.Vehicles.Any())
            {
                // --- Iron Alliance: 1x L1 + 3*(L2..L4)
                var iaL1 = new Vehicle
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

                // Scout L2..L4 (Iron) – швидкі, тонші
                var iaS2 = new Vehicle
                {
                    Code = "ia_scout_l2",
                    Name = "IA Skirmisher L2",
                    FactionId = iron.Id,
                    Branch = "tracked",
                    Class = VehicleClass.Scout,
                    Level = 2,
                    PurchaseCost = 8000,

                    HP = 140, Damage = 16, Penetration = 55,
                    ReloadTime = 2.4f, Accuracy = 0.86f, AimTime = 1.7f,
                    Speed = 6.5f, Acceleration = 3.8f, TraverseSpeed = 36f, TurretTraverseSpeed = 32f,
                    TurretArmorFront = 55, TurretArmorSide = 30, TurretArmorRear = 22,
                    HullArmorFront = 60, HullArmorSide = 35, HullArmorRear = 28
                };
                var iaS3 = new Vehicle
                {
                    Code = "ia_scout_l3",
                    Name = "IA Skirmisher L3",
                    FactionId = iron.Id,
                    Branch = "tracked",
                    Class = VehicleClass.Scout,
                    Level = 3,
                    PurchaseCost = 14000,

                    HP = 160, Damage = 20, Penetration = 70,
                    ReloadTime = 2.3f, Accuracy = 0.88f, AimTime = 1.6f,
                    Speed = 6.8f, Acceleration = 4.0f, TraverseSpeed = 38f, TurretTraverseSpeed = 34f,
                    TurretArmorFront = 65, TurretArmorSide = 36, TurretArmorRear = 26,
                    HullArmorFront = 72, HullArmorSide = 42, HullArmorRear = 32
                };
                var iaS4 = new Vehicle
                {
                    Code = "ia_scout_l4",
                    Name = "IA Skirmisher L4",
                    FactionId = iron.Id,
                    Branch = "tracked",
                    Class = VehicleClass.Scout,
                    Level = 4,
                    PurchaseCost = 21000,

                    HP = 180, Damage = 24, Penetration = 85,
                    ReloadTime = 2.2f, Accuracy = 0.90f, AimTime = 1.5f,
                    Speed = 7.2f, Acceleration = 4.3f, TraverseSpeed = 40f, TurretTraverseSpeed = 36f,
                    TurretArmorFront = 75, TurretArmorSide = 42, TurretArmorRear = 30,
                    HullArmorFront = 82, HullArmorSide = 48, HullArmorRear = 36
                };

                // Guardian L2..L4 (Iron) – баланс
                var iaG2 = new Vehicle
                {
                    Code = "ia_guardian_l2",
                    Name = "IA Strider L2",
                    FactionId = iron.Id,
                    Branch = "biped",
                    Class = VehicleClass.Guardian,
                    Level = 2,
                    PurchaseCost = 9000,

                    HP = 180, Damage = 18, Penetration = 80,
                    ReloadTime = 3.2f, Accuracy = 0.82f, AimTime = 2.2f,
                    Speed = 5.0f, Acceleration = 3.0f, TraverseSpeed = 32f, TurretTraverseSpeed = 28f,
                    TurretArmorFront = 90, TurretArmorSide = 60, TurretArmorRear = 45,
                    HullArmorFront = 110, HullArmorSide = 70, HullArmorRear = 55
                };
                var iaG3 = new Vehicle
                {
                    Code = "ia_guardian_l3",
                    Name = "IA Strider L3",
                    FactionId = iron.Id,
                    Branch = "biped",
                    Class = VehicleClass.Guardian,
                    Level = 3,
                    PurchaseCost = 15500,

                    HP = 210, Damage = 22, Penetration = 95,
                    ReloadTime = 3.0f, Accuracy = 0.84f, AimTime = 2.0f,
                    Speed = 5.2f, Acceleration = 3.2f, TraverseSpeed = 33f, TurretTraverseSpeed = 29f,
                    TurretArmorFront = 105, TurretArmorSide = 72, TurretArmorRear = 54,
                    HullArmorFront = 126, HullArmorSide = 82, HullArmorRear = 64
                };
                var iaG4 = new Vehicle
                {
                    Code = "ia_guardian_l4",
                    Name = "IA Strider L4",
                    FactionId = iron.Id,
                    Branch = "biped",
                    Class = VehicleClass.Guardian,
                    Level = 4,
                    PurchaseCost = 23000,

                    HP = 240, Damage = 26, Penetration = 110,
                    ReloadTime = 2.8f, Accuracy = 0.86f, AimTime = 1.9f,
                    Speed = 5.4f, Acceleration = 3.4f, TraverseSpeed = 34f, TurretTraverseSpeed = 30f,
                    TurretArmorFront = 120, TurretArmorSide = 84, TurretArmorRear = 63,
                    HullArmorFront = 140, HullArmorSide = 94, HullArmorRear = 74
                };

                // Colossus L2..L4 (Iron) – повільні, дуже міцні
                var iaC2 = new Vehicle
                {
                    Code = "ia_colossus_l2",
                    Name = "IA Bastion L2",
                    FactionId = iron.Id,
                    Branch = "tracked",
                    Class = VehicleClass.Colossus,
                    Level = 2,
                    PurchaseCost = 12000,

                    HP = 320, Damage = 30, Penetration = 110,
                    ReloadTime = 4.5f, Accuracy = 0.75f, AimTime = 2.8f,
                    Speed = 2.0f, Acceleration = 1.5f, TraverseSpeed = 20f, TurretTraverseSpeed = 18f,
                    TurretArmorFront = 200, TurretArmorSide = 120, TurretArmorRear = 80,
                    HullArmorFront = 220, HullArmorSide = 140, HullArmorRear = 100
                };
                var iaC3 = new Vehicle
                {
                    Code = "ia_colossus_l3",
                    Name = "IA Bastion L3",
                    FactionId = iron.Id,
                    Branch = "tracked",
                    Class = VehicleClass.Colossus,
                    Level = 3,
                    PurchaseCost = 18500,

                    HP = 360, Damage = 34, Penetration = 125,
                    ReloadTime = 4.3f, Accuracy = 0.77f, AimTime = 2.6f,
                    Speed = 2.2f, Acceleration = 1.6f, TraverseSpeed = 21f, TurretTraverseSpeed = 19f,
                    TurretArmorFront = 220, TurretArmorSide = 135, TurretArmorRear = 92,
                    HullArmorFront = 240, HullArmorSide = 155, HullArmorRear = 112
                };
                var iaC4 = new Vehicle
                {
                    Code = "ia_colossus_l4",
                    Name = "IA Bastion L4",
                    FactionId = iron.Id,
                    Branch = "tracked",
                    Class = VehicleClass.Colossus,
                    Level = 4,
                    PurchaseCost = 26000,

                    HP = 400, Damage = 38, Penetration = 140,
                    ReloadTime = 4.1f, Accuracy = 0.79f, AimTime = 2.5f,
                    Speed = 2.4f, Acceleration = 1.7f, TraverseSpeed = 22f, TurretTraverseSpeed = 20f,
                    TurretArmorFront = 240, TurretArmorSide = 150, TurretArmorRear = 104,
                    HullArmorFront = 260, HullArmorSide = 170, HullArmorRear = 124
                };

                // --- Nova Syndicate: 1x L1 + 3*(L2..L4)
                var nvL1 = new Vehicle
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

                // Scout L2..L4 (Nova)
                var nvS2 = new Vehicle
                {
                    Code = "nv_scout_l2",
                    Name = "Nova Wisp L2",
                    FactionId = nova.Id,
                    Branch = "biped",
                    Class = VehicleClass.Scout,
                    Level = 2,
                    PurchaseCost = 8200,

                    HP = 120, Damage = 17, Penetration = 72,
                    ReloadTime = 2.1f, Accuracy = 0.88f, AimTime = 1.5f,
                    Speed = 7.4f, Acceleration = 4.3f, TraverseSpeed = 40f, TurretTraverseSpeed = 36f,
                    TurretArmorFront = 45, TurretArmorSide = 28, TurretArmorRear = 22,
                    HullArmorFront = 50, HullArmorSide = 32, HullArmorRear = 26
                };
                var nvS3 = new Vehicle
                {
                    Code = "nv_scout_l3",
                    Name = "Nova Wisp L3",
                    FactionId = nova.Id,
                    Branch = "biped",
                    Class = VehicleClass.Scout,
                    Level = 3,
                    PurchaseCost = 14500,

                    HP = 140, Damage = 20, Penetration = 85,
                    ReloadTime = 2.0f, Accuracy = 0.90f, AimTime = 1.4f,
                    Speed = 7.8f, Acceleration = 4.6f, TraverseSpeed = 42f, TurretTraverseSpeed = 38f,
                    TurretArmorFront = 55, TurretArmorSide = 34, TurretArmorRear = 26,
                    HullArmorFront = 58, HullArmorSide = 36, HullArmorRear = 30
                };
                var nvS4 = new Vehicle
                {
                    Code = "nv_scout_l4",
                    Name = "Nova Wisp L4",
                    FactionId = nova.Id,
                    Branch = "biped",
                    Class = VehicleClass.Scout,
                    Level = 4,
                    PurchaseCost = 21500,

                    HP = 160, Damage = 24, Penetration = 98,
                    ReloadTime = 1.9f, Accuracy = 0.92f, AimTime = 1.3f,
                    Speed = 8.2f, Acceleration = 4.9f, TraverseSpeed = 44f, TurretTraverseSpeed = 40f,
                    TurretArmorFront = 65, TurretArmorSide = 40, TurretArmorRear = 30,
                    HullArmorFront = 66, HullArmorSide = 42, HullArmorRear = 34
                };

                // Guardian L2..L4 (Nova)
                var nvG2 = new Vehicle
                {
                    Code = "nv_guardian_l2",
                    Name = "Nova Raptor L2",
                    FactionId = nova.Id,
                    Branch = "biped",
                    Class = VehicleClass.Guardian,
                    Level = 2,
                    PurchaseCost = 9800,

                    HP = 160, Damage = 22, Penetration = 85,
                    ReloadTime = 3.0f, Accuracy = 0.84f, AimTime = 2.0f,
                    Speed = 6.0f, Acceleration = 3.6f, TraverseSpeed = 34f, TurretTraverseSpeed = 30f,
                    TurretArmorFront = 80, TurretArmorSide = 55, TurretArmorRear = 40,
                    HullArmorFront = 95, HullArmorSide = 65, HullArmorRear = 48
                };
                var nvG3 = new Vehicle
                {
                    Code = "nv_guardian_l3",
                    Name = "Nova Raptor L3",
                    FactionId = nova.Id,
                    Branch = "biped",
                    Class = VehicleClass.Guardian,
                    Level = 3,
                    PurchaseCost = 15800,

                    HP = 185, Damage = 25, Penetration = 98,
                    ReloadTime = 2.8f, Accuracy = 0.86f, AimTime = 1.9f,
                    Speed = 6.2f, Acceleration = 3.8f, TraverseSpeed = 35f, TurretTraverseSpeed = 31f,
                    TurretArmorFront = 92, TurretArmorSide = 64, TurretArmorRear = 48,
                    HullArmorFront = 108, HullArmorSide = 74, HullArmorRear = 56
                };
                var nvG4 = new Vehicle
                {
                    Code = "nv_guardian_l4",
                    Name = "Nova Raptor L4",
                    FactionId = nova.Id,
                    Branch = "biped",
                    Class = VehicleClass.Guardian,
                    Level = 4,
                    PurchaseCost = 23200,

                    HP = 210, Damage = 28, Penetration = 112,
                    ReloadTime = 2.6f, Accuracy = 0.88f, AimTime = 1.8f,
                    Speed = 6.4f, Acceleration = 4.0f, TraverseSpeed = 36f, TurretTraverseSpeed = 32f,
                    TurretArmorFront = 104, TurretArmorSide = 72, TurretArmorRear = 56,
                    HullArmorFront = 120, HullArmorSide = 82, HullArmorRear = 64
                };

                // Colossus L2..L4 (Nova) – важкі, tracked
                var nvC2 = new Vehicle
                {
                    Code = "nv_colossus_l2",
                    Name = "Nova Bulwark L2",
                    FactionId = nova.Id,
                    Branch = "tracked",
                    Class = VehicleClass.Colossus,
                    Level = 2,
                    PurchaseCost = 11500,

                    HP = 280, Damage = 26, Penetration = 100,
                    ReloadTime = 4.0f, Accuracy = 0.78f, AimTime = 2.6f,
                    Speed = 3.0f, Acceleration = 1.8f, TraverseSpeed = 22f, TurretTraverseSpeed = 20f,
                    TurretArmorFront = 180, TurretArmorSide = 110, TurretArmorRear = 70,
                    HullArmorFront = 200, HullArmorSide = 120, HullArmorRear = 85
                };
                var nvC3 = new Vehicle
                {
                    Code = "nv_colossus_l3",
                    Name = "Nova Bulwark L3",
                    FactionId = nova.Id,
                    Branch = "tracked",
                    Class = VehicleClass.Colossus,
                    Level = 3,
                    PurchaseCost = 17800,

                    HP = 315, Damage = 30, Penetration = 114,
                    ReloadTime = 3.9f, Accuracy = 0.80f, AimTime = 2.5f,
                    Speed = 3.2f, Acceleration = 1.9f, TraverseSpeed = 23f, TurretTraverseSpeed = 21f,
                    TurretArmorFront = 196, TurretArmorSide = 124, TurretArmorRear = 82,
                    HullArmorFront = 216, HullArmorSide = 134, HullArmorRear = 98
                };
                var nvC4 = new Vehicle
                {
                    Code = "nv_colossus_l4",
                    Name = "Nova Bulwark L4",
                    FactionId = nova.Id,
                    Branch = "tracked",
                    Class = VehicleClass.Colossus,
                    Level = 4,
                    PurchaseCost = 25000,

                    HP = 350, Damage = 34, Penetration = 128,
                    ReloadTime = 3.8f, Accuracy = 0.82f, AimTime = 2.4f,
                    Speed = 3.4f, Acceleration = 2.0f, TraverseSpeed = 24f, TurretTraverseSpeed = 22f,
                    TurretArmorFront = 212, TurretArmorSide = 136, TurretArmorRear = 94,
                    HullArmorFront = 232, HullArmorSide = 146, HullArmorRear = 110
                };

                db.Vehicles.AddRange(
                    iaL1, iaS2, iaS3, iaS4, iaG2, iaG3, iaG4, iaC2, iaC3, iaC4,
                    nvL1, nvS2, nvS3, nvS4, nvG2, nvG3, nvG4, nvC2, nvC3, nvC4
                );
                db.SaveChanges();

                // Research links
                if (!db.VehicleResearchRequirements.Any())
                {
                    db.VehicleResearchRequirements.AddRange(
                        // Iron: L1 -> any L2
                        new VehicleResearchRequirement { PredecessorVehicleId = iaL1.Id, SuccessorVehicleId = iaS2.Id, RequiredXpOnPredecessor = 1500 },
                        new VehicleResearchRequirement { PredecessorVehicleId = iaL1.Id, SuccessorVehicleId = iaG2.Id, RequiredXpOnPredecessor = 1600 },
                        new VehicleResearchRequirement { PredecessorVehicleId = iaL1.Id, SuccessorVehicleId = iaC2.Id, RequiredXpOnPredecessor = 2200 },
                        // Iron: class lines
                        new VehicleResearchRequirement { PredecessorVehicleId = iaS2.Id, SuccessorVehicleId = iaS3.Id, RequiredXpOnPredecessor = 4200 },
                        new VehicleResearchRequirement { PredecessorVehicleId = iaS3.Id, SuccessorVehicleId = iaS4.Id, RequiredXpOnPredecessor = 7600 },
                        new VehicleResearchRequirement { PredecessorVehicleId = iaG2.Id, SuccessorVehicleId = iaG3.Id, RequiredXpOnPredecessor = 4800 },
                        new VehicleResearchRequirement { PredecessorVehicleId = iaG3.Id, SuccessorVehicleId = iaG4.Id, RequiredXpOnPredecessor = 8200 },
                        new VehicleResearchRequirement { PredecessorVehicleId = iaC2.Id, SuccessorVehicleId = iaC3.Id, RequiredXpOnPredecessor = 5200 },
                        new VehicleResearchRequirement { PredecessorVehicleId = iaC3.Id, SuccessorVehicleId = iaC4.Id, RequiredXpOnPredecessor = 9000 },

                        // Nova: L1 -> any L2
                        new VehicleResearchRequirement { PredecessorVehicleId = nvL1.Id, SuccessorVehicleId = nvS2.Id, RequiredXpOnPredecessor = 1500 },
                        new VehicleResearchRequirement { PredecessorVehicleId = nvL1.Id, SuccessorVehicleId = nvG2.Id, RequiredXpOnPredecessor = 1600 },
                        new VehicleResearchRequirement { PredecessorVehicleId = nvL1.Id, SuccessorVehicleId = nvC2.Id, RequiredXpOnPredecessor = 2200 },
                        // Nova: class lines
                        new VehicleResearchRequirement { PredecessorVehicleId = nvS2.Id, SuccessorVehicleId = nvS3.Id, RequiredXpOnPredecessor = 4300 },
                        new VehicleResearchRequirement { PredecessorVehicleId = nvS3.Id, SuccessorVehicleId = nvS4.Id, RequiredXpOnPredecessor = 7800 },
                        new VehicleResearchRequirement { PredecessorVehicleId = nvG2.Id, SuccessorVehicleId = nvG3.Id, RequiredXpOnPredecessor = 4900 },
                        new VehicleResearchRequirement { PredecessorVehicleId = nvG3.Id, SuccessorVehicleId = nvG4.Id, RequiredXpOnPredecessor = 8400 },
                        new VehicleResearchRequirement { PredecessorVehicleId = nvC2.Id, SuccessorVehicleId = nvC3.Id, RequiredXpOnPredecessor = 5300 },
                        new VehicleResearchRequirement { PredecessorVehicleId = nvC3.Id, SuccessorVehicleId = nvC4.Id, RequiredXpOnPredecessor = 9200 }
                    );
                    db.SaveChanges();
                }
            }

            // Players
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

                var starter = db.Vehicles.First(v => v.Code == "ia_l1_starter");
                db.UserVehicles.Add(new UserVehicle { UserId = user.Id, VehicleId = starter.Id, IsActive = true });
                db.SaveChanges();
            }

            // Demo Match
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
