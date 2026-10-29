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
                    existing.TraverseAcceleration = v.TraverseAcceleration;
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

                HP = 105, ShellSpeed = 126f, ShellsCount = 87, DamageMin = 52.8f, DamageMax = 79.2f, Penetration = 99, ViewRange = 105f,
                ReloadTime = 2.2f, Accuracy = 1.13f, AimTime = 1.75f,
                Speed = 3.84f, Acceleration = 0.9f, TraverseAcceleration = 90f, TraverseSpeed = 34f, TurretTraverseSpeed = 19.5f,
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
                HP = 180, ShellSpeed = 144f, ShellsCount = 149, DamageMin = 25.2f, DamageMax = 37.8f, Penetration = 90, ViewRange = 125f,
                ReloadTime = 1.25f, Accuracy = 0.88f, AimTime = 1.15f,
                Speed = 5.616f, Acceleration = 1.0f, TraverseAcceleration = 100f, TraverseSpeed = 39f, TurretTraverseSpeed = 24f,
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
                HP = 250, ShellSpeed = 111f, ShellsCount = 65, DamageMin = 86.4f, DamageMax = 129.6f, Penetration = 115, ViewRange = 100f,
                ReloadTime = 3.0f, Accuracy = 1.4f, AimTime = 2.35f,
                Speed = 3.54f, Acceleration = 0.775f, TraverseAcceleration = 77.5f, TraverseSpeed = 33f, TurretTraverseSpeed = 18.75f,
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
                HP = 340, ShellSpeed = 90f, ShellsCount = 50, DamageMin = 115.2f, DamageMax = 172.8f, Penetration = 135, ViewRange = 85f,
                ReloadTime = 4.0f, Accuracy = 2.0f, AimTime = 3.3f,
                Speed = 2.82f, Acceleration = 0.6f, TraverseAcceleration = 60f, TraverseSpeed = 27f, TurretTraverseSpeed = 11.25f,
                TurretArmorFront = 116, TurretArmorSide = 72, TurretArmorRear = 50,
                HullArmorFront = 102, HullArmorSide = 66, HullArmorRear = 46,
                IsVisible = true
            });

            // Links: L1 -> (Scout|Guardian|Colossus)
            EnsureLink(iaStarter.Id, iaL2Scout.Id,    requiredXp: 400);
            EnsureLink(iaStarter.Id, iaL2Guardian.Id, requiredXp: 700);
            EnsureLink(iaStarter.Id, iaL2Colossus.Id, requiredXp: 1000);

            var iaL3ScoutWraith = EnsureVehicle(new Vehicle
            {
                Code = "ia_l3_scout_wraith",
                Name = "IA Wraith",
                FactionId = iron.Id,
                Branch = "tracked",
                Class = VehicleClass.Scout,
                Level = 3,
                PurchaseCost = 22000,
                HP = 230, ShellSpeed = 150f, ShellsCount = 170, DamageMin = 34.8f, DamageMax = 52.2f, Penetration = 102, ViewRange = 135f,
                ReloadTime = 1.1f, Accuracy = 0.82f, AimTime = 0.95f,
                Speed = 5.95f, Acceleration = 1.0f, TraverseAcceleration = 100f, TraverseSpeed = 42f, TurretTraverseSpeed = 25f,
                TurretArmorFront = 58, TurretArmorSide = 36, TurretArmorRear = 25,
                HullArmorFront = 53, HullArmorSide = 33, HullArmorRear = 23,
                IsVisible = false
            });

            var iaL3GuardianWarder = EnsureVehicle(new Vehicle
            {
                Code = "ia_l3_guardian_warder",
                Name = "IA Warder",
                FactionId = iron.Id,
                Branch = "tracked",
                Class = VehicleClass.Guardian,
                Level = 3,
                PurchaseCost = 24000,
                HP = 320, ShellSpeed = 120f, ShellsCount = 90, DamageMin = 100.8f, DamageMax = 151.2f, Penetration = 125, ViewRange = 108f,
                ReloadTime = 2.8f, Accuracy = 1.45f, AimTime = 2.15f,
                Speed = 3.75f, Acceleration = 0.85f, TraverseAcceleration = 85f, TraverseSpeed = 35f, TurretTraverseSpeed = 21f,
                TurretArmorFront = 88, TurretArmorSide = 56, TurretArmorRear = 38,
                HullArmorFront = 80, HullArmorSide = 52, HullArmorRear = 36,
                IsVisible = false
            });

            var iaL3GuardianRampart = EnsureVehicle(new Vehicle
            {
                Code = "ia_l3_guardian_rampart",
                Name = "IA Rampart",
                FactionId = iron.Id,
                Branch = "tracked",
                Class = VehicleClass.Guardian,
                Level = 3,
                PurchaseCost = 27000,
                HP = 360, ShellSpeed = 114f, ShellsCount = 80, DamageMin = 108f, DamageMax = 162f, Penetration = 130, ViewRange = 102f,
                ReloadTime = 3.0f, Accuracy = 1.55f, AimTime = 2.3f,
                Speed = 3.45f, Acceleration = 0.8f, TraverseAcceleration = 80f, TraverseSpeed = 33.5f, TurretTraverseSpeed = 20f,
                TurretArmorFront = 100, TurretArmorSide = 64, TurretArmorRear = 43,
                HullArmorFront = 91, HullArmorSide = 59, HullArmorRear = 40,
                IsVisible = false
            });

            var iaL3ColossusMastodon = EnsureVehicle(new Vehicle
            {
                Code = "ia_l3_colossus_mastodon",
                Name = "IA Mastodon",
                FactionId = iron.Id,
                Branch = "tracked",
                Class = VehicleClass.Colossus,
                Level = 3,
                PurchaseCost = 36000,
                HP = 430, ShellSpeed = 96f, ShellsCount = 58, DamageMin = 145.2f, DamageMax = 217.8f, Penetration = 145, ViewRange = 88f,
                ReloadTime = 4.5f, Accuracy = 1.9f, AimTime = 3.6f,
                Speed = 2.95f, Acceleration = 0.65f, TraverseAcceleration = 65f, TraverseSpeed = 28f, TurretTraverseSpeed = 12.2f,
                TurretArmorFront = 130, TurretArmorSide = 82, TurretArmorRear = 56,
                HullArmorFront = 116, HullArmorSide = 75, HullArmorRear = 52,
                IsVisible = false
            });

            var iaL4GuardianCitadel = EnsureVehicle(new Vehicle
            {
                Code = "ia_l4_guardian_citadel",
                Name = "IA Citadel",
                FactionId = iron.Id,
                Branch = "tracked",
                Class = VehicleClass.Guardian,
                Level = 4,
                PurchaseCost = 44000,
                HP = 445, ShellSpeed = 126f, ShellsCount = 95, DamageMin = 120f, DamageMax = 180f, Penetration = 140, ViewRange = 110f,
                ReloadTime = 2.75f, Accuracy = 1.6f, AimTime = 2.05f,
                Speed = 3.6f, Acceleration = 0.825f, TraverseAcceleration = 82.5f, TraverseSpeed = 35f, TurretTraverseSpeed = 20.5f,
                TurretArmorFront = 118, TurretArmorSide = 75, TurretArmorRear = 50,
                HullArmorFront = 108, HullArmorSide = 70, HullArmorRear = 48,
                IsVisible = false
            });

            var iaL4ColossusDreadnought = EnsureVehicle(new Vehicle
            {
                Code = "ia_l4_colossus_dreadnought",
                Name = "IA Dreadnought",
                FactionId = iron.Id,
                Branch = "tracked",
                Class = VehicleClass.Colossus,
                Level = 4,
                PurchaseCost = 56000,
                HP = 560, ShellSpeed = 92f, ShellsCount = 52, DamageMin = 172.8f, DamageMax = 259.2f, Penetration = 155, ViewRange = 90f,
                ReloadTime = 4.8f, Accuracy = 2.0f, AimTime = 3.9f,
                Speed = 2.6f, Acceleration = 0.55f, TraverseAcceleration = 55f, TraverseSpeed = 25.5f, TurretTraverseSpeed = 10.5f,
                TurretArmorFront = 160, TurretArmorSide = 100, TurretArmorRear = 68,
                HullArmorFront = 144, HullArmorSide = 93, HullArmorRear = 64,
                IsVisible = false
            });

            EnsureLink(iaL2Scout.Id, iaL3ScoutWraith.Id, requiredXp: 1800);
            EnsureLink(iaL2Guardian.Id, iaL3GuardianWarder.Id, requiredXp: 2200);
            EnsureLink(iaL2Guardian.Id, iaL3GuardianRampart.Id, requiredXp: 2600);
            EnsureLink(iaL2Colossus.Id, iaL3ColossusMastodon.Id, requiredXp: 3200);
            EnsureLink(iaL3GuardianWarder.Id, iaL4GuardianCitadel.Id, requiredXp: 5000);
            EnsureLink(iaL3ColossusMastodon.Id, iaL4ColossusDreadnought.Id, requiredXp: 6200);

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

                HP = 100, ShellSpeed = 135f, ShellsCount = 118, DamageMin = 38.4f, DamageMax = 57.6f, Penetration = 102, ViewRange = 110f,
                ReloadTime = 1.6f, Accuracy = 1.02f, AimTime = 1.45f,
                Speed = 1.62f, Acceleration = 0.375f, TraverseAcceleration = 37.5f, TraverseSpeed = 62f, TurretTraverseSpeed = 56.95f,
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
                HP = 180, ShellSpeed = 150f, ShellsCount = 185, DamageMin = 28.8f, DamageMax = 43.2f, Penetration = 96, ViewRange = 130f,
                ReloadTime = 1.0f, Accuracy = 0.8f, AimTime = 0.95f,
                Speed = 1.86f, Acceleration = 0.4f, TraverseAcceleration = 40f, TraverseSpeed = 72f, TurretTraverseSpeed = 68f,
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
                HP = 250, ShellSpeed = 120f, ShellsCount = 75, DamageMin = 74.4f, DamageMax = 112.8f, Penetration = 119, ViewRange = 105f,
                ReloadTime = 2.6f, Accuracy = 1.26f, AimTime = 2.05f,
                Speed = 1.56f, Acceleration = 0.325f, TraverseAcceleration = 32.5f, TraverseSpeed = 66f, TurretTraverseSpeed = 60.35f,
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
                HP = 335, ShellSpeed = 99f, ShellsCount = 57, DamageMin = 100.8f, DamageMax = 151.2f, Penetration = 132, ViewRange = 90f,
                ReloadTime = 3.5f, Accuracy = 1.81f, AimTime = 2.9f,
                Speed = 1.26f, Acceleration = 0.275f, TraverseAcceleration = 27.5f, TraverseSpeed = 60f, TurretTraverseSpeed = 51.85f,
                TurretArmorFront = 106, TurretArmorSide = 70, TurretArmorRear = 48,
                HullArmorFront = 94, HullArmorSide = 64, HullArmorRear = 45,
                IsVisible = true
            });

            // Links: L1 -> (Scout|Guardian|Colossus)
            EnsureLink(nvStarter.Id, nvL2Scout.Id,    requiredXp: 400);
            EnsureLink(nvStarter.Id, nvL2Guardian.Id, requiredXp: 700);
            EnsureLink(nvStarter.Id, nvL2Colossus.Id, requiredXp: 1000);

            var nvL3ScoutRift = EnsureVehicle(new Vehicle
            {
                Code = "nv_l3_scout_rift",
                Name = "Nova Rift",
                FactionId = nova.Id,
                Branch = "biped",
                Class = VehicleClass.Scout,
                Level = 3,
                PurchaseCost = 22000,
                HP = 220, ShellSpeed = 150f, ShellsCount = 205, DamageMin = 33.6f, DamageMax = 50.4f, Penetration = 104, ViewRange = 140f,
                ReloadTime = 0.95f, Accuracy = 0.8f, AimTime = 0.85f,
                Speed = 1.98f, Acceleration = 0.425f, TraverseAcceleration = 42.5f, TraverseSpeed = 76f, TurretTraverseSpeed = 72f,
                TurretArmorFront = 54, TurretArmorSide = 34, TurretArmorRear = 23,
                HullArmorFront = 49, HullArmorSide = 31, HullArmorRear = 21,
                IsVisible = false
            });

            var nvL3ScoutSpecter = EnsureVehicle(new Vehicle
            {
                Code = "nv_l3_scout_specter",
                Name = "Nova Specter",
                FactionId = nova.Id,
                Branch = "biped",
                Class = VehicleClass.Scout,
                Level = 3,
                PurchaseCost = 25000,
                HP = 205, ShellSpeed = 150f, ShellsCount = 220, DamageMin = 30f, DamageMax = 45f, Penetration = 101, ViewRange = 145f,
                ReloadTime = 0.85f, Accuracy = 0.8f, AimTime = 0.8f,
                Speed = 2.1f, Acceleration = 0.45f, TraverseAcceleration = 45f, TraverseSpeed = 78f, TurretTraverseSpeed = 74f,
                TurretArmorFront = 50, TurretArmorSide = 32, TurretArmorRear = 22,
                HullArmorFront = 46, HullArmorSide = 29, HullArmorRear = 20,
                IsVisible = false
            });

            var nvL4ScoutHorizon = EnsureVehicle(new Vehicle
            {
                Code = "nv_l4_scout_horizon",
                Name = "Nova Horizon",
                FactionId = nova.Id,
                Branch = "biped",
                Class = VehicleClass.Scout,
                Level = 4,
                PurchaseCost = 47000,
                HP = 275, ShellSpeed = 150f, ShellsCount = 235, DamageMin = 36f, DamageMax = 54f, Penetration = 110, ViewRange = 155f,
                ReloadTime = 0.85f, Accuracy = 0.8f, AimTime = 0.75f,
                Speed = 2.28f, Acceleration = 0.475f, TraverseAcceleration = 47.5f, TraverseSpeed = 82f, TurretTraverseSpeed = 78f,
                TurretArmorFront = 65, TurretArmorSide = 41, TurretArmorRear = 28,
                HullArmorFront = 59, HullArmorSide = 38, HullArmorRear = 26,
                IsVisible = false
            });

            var nvL3GuardianPrism = EnsureVehicle(new Vehicle
            {
                Code = "nv_l3_guardian_prism",
                Name = "Nova Prism",
                FactionId = nova.Id,
                Branch = "biped",
                Class = VehicleClass.Guardian,
                Level = 3,
                PurchaseCost = 26000,
                HP = 315, ShellSpeed = 132f, ShellsCount = 95, DamageMin = 90f, DamageMax = 135f, Penetration = 130, ViewRange = 115f,
                ReloadTime = 2.4f, Accuracy = 1.15f, AimTime = 1.8f,
                Speed = 1.74f, Acceleration = 0.36f, TraverseAcceleration = 36f, TraverseSpeed = 70f, TurretTraverseSpeed = 64f,
                TurretArmorFront = 94, TurretArmorSide = 60, TurretArmorRear = 40,
                HullArmorFront = 84, HullArmorSide = 55, HullArmorRear = 37,
                IsVisible = false
            });

            var nvL3ColossusHarrow = EnsureVehicle(new Vehicle
            {
                Code = "nv_l3_colossus_harrow",
                Name = "Nova Harrow",
                FactionId = nova.Id,
                Branch = "biped",
                Class = VehicleClass.Colossus,
                Level = 3,
                PurchaseCost = 35000,
                HP = 420, ShellSpeed = 105f, ShellsCount = 62, DamageMin = 115.2f, DamageMax = 172.8f, Penetration = 142, ViewRange = 94f,
                ReloadTime = 3.3f, Accuracy = 1.75f, AimTime = 2.65f,
                Speed = 1.45f, Acceleration = 0.3f, TraverseAcceleration = 30f, TraverseSpeed = 56f, TurretTraverseSpeed = 48f,
                TurretArmorFront = 120, TurretArmorSide = 78, TurretArmorRear = 53,
                HullArmorFront = 108, HullArmorSide = 70, HullArmorRear = 48,
                IsVisible = false
            });

            var nvL3ColossusOblivion = EnsureVehicle(new Vehicle
            {
                Code = "nv_l3_colossus_oblivion",
                Name = "Nova Oblivion",
                FactionId = nova.Id,
                Branch = "biped",
                Class = VehicleClass.Colossus,
                Level = 3,
                PurchaseCost = 39000,
                HP = 470, ShellSpeed = 99f, ShellsCount = 55, DamageMin = 129.6f, DamageMax = 194.4f, Penetration = 148, ViewRange = 88f,
                ReloadTime = 3.6f, Accuracy = 1.9f, AimTime = 2.9f,
                Speed = 1.34f, Acceleration = 0.285f, TraverseAcceleration = 28.5f, TraverseSpeed = 53f, TurretTraverseSpeed = 46f,
                TurretArmorFront = 136, TurretArmorSide = 88, TurretArmorRear = 60,
                HullArmorFront = 122, HullArmorSide = 79, HullArmorRear = 54,
                IsVisible = false
            });

            var nvL4ColossusEclipse = EnsureVehicle(new Vehicle
            {
                Code = "nv_l4_colossus_eclipse",
                Name = "Nova Eclipse",
                FactionId = nova.Id,
                Branch = "biped",
                Class = VehicleClass.Colossus,
                Level = 4,
                PurchaseCost = 60000,
                HP = 575, ShellSpeed = 93f, ShellsCount = 50, DamageMin = 151.2f, DamageMax = 226.8f, Penetration = 158, ViewRange = 92f,
                ReloadTime = 3.8f, Accuracy = 2.0f, AimTime = 3.05f,
                Speed = 1.2f, Acceleration = 0.26f, TraverseAcceleration = 26f, TraverseSpeed = 50f, TurretTraverseSpeed = 44f,
                TurretArmorFront = 160, TurretArmorSide = 103, TurretArmorRear = 70,
                HullArmorFront = 143, HullArmorSide = 94, HullArmorRear = 64,
                IsVisible = false
            });

            EnsureLink(nvL2Scout.Id, nvL3ScoutRift.Id, requiredXp: 1700);
            EnsureLink(nvL2Scout.Id, nvL3ScoutSpecter.Id, requiredXp: 2200);
            EnsureLink(nvL3ScoutRift.Id, nvL4ScoutHorizon.Id, requiredXp: 4800);
            EnsureLink(nvL2Guardian.Id, nvL3GuardianPrism.Id, requiredXp: 2400);
            EnsureLink(nvL2Colossus.Id, nvL3ColossusHarrow.Id, requiredXp: 3000);
            EnsureLink(nvL2Colossus.Id, nvL3ColossusOblivion.Id, requiredXp: 3600);
            EnsureLink(nvL3ColossusHarrow.Id, nvL4ColossusEclipse.Id, requiredXp: 6500);

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
