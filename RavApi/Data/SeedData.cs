using System;
using System.Linq;
using RavApi.Models;

namespace RavApi.Data
{
    public static class SeedData
    {
        public static void Initialize(AppDbContext db, bool includeDevelopmentFixtures)
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
                    existing.EnginePower = v.EnginePower;
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

            void SyncLinks(IEnumerable<Vehicle> vehicles, params (Vehicle Predecessor, Vehicle Successor, int RequiredXp)[] desiredLinks)
            {
                var vehicleIds = vehicles.Select(vehicle => vehicle.Id).ToHashSet();
                var desired = desiredLinks.ToDictionary(
                    link => (PredecessorId: link.Predecessor.Id, SuccessorId: link.Successor.Id),
                    link => link.RequiredXp);
                var existing = db.VehicleResearchRequirements
                    .Where(link => vehicleIds.Contains(link.PredecessorVehicleId)
                                   && vehicleIds.Contains(link.SuccessorVehicleId))
                    .ToList();

                foreach (var link in existing)
                {
                    var key = (PredecessorId: link.PredecessorVehicleId, SuccessorId: link.SuccessorVehicleId);
                    if (!desired.TryGetValue(key, out var requiredXp))
                    {
                        db.VehicleResearchRequirements.Remove(link);
                        continue;
                    }

                    link.RequiredXpOnPredecessor = requiredXp;
                }

                var existingKeys = existing
                    .Select(link => (PredecessorId: link.PredecessorVehicleId, SuccessorId: link.SuccessorVehicleId))
                    .ToHashSet();
                foreach (var link in desiredLinks)
                {
                    var key = (PredecessorId: link.Predecessor.Id, SuccessorId: link.Successor.Id);
                    if (existingKeys.Contains(key))
                    {
                        continue;
                    }

                    db.VehicleResearchRequirements.Add(new VehicleResearchRequirement
                    {
                        PredecessorVehicleId = key.PredecessorId,
                        SuccessorVehicleId = key.SuccessorId,
                        RequiredXpOnPredecessor = link.RequiredXp
                    });
                }

                db.SaveChanges();
            }

            // --- Vehicles (ідемпотентно; без if (!db.Vehicles.Any())) ---

            // Iron Alliance (tracked) — L1 + три L2
            var iaStarter = EnsureVehicle(new Vehicle
            {
                Code = "ia_l1_starter",
                Name = "TZ-18",
                FactionId = iron.Id,
                Branch = "tracked",
                Class = VehicleClass.Scout,
                Level = 1,
                PurchaseCost = 0,

                HP = 105, ShellSpeed = 126f, ShellsCount = 87, DamageMin = 52.8f, DamageMax = 79.2f, Penetration = 99, ViewRange = 105f,
                ReloadTime = 2.2f, Accuracy = 1.13f, AimTime = 1.75f,
                Speed = 3.84f, Acceleration = 0.9f, EnginePower = 700f, TraverseAcceleration = 90f, TraverseSpeed = 34f, TurretTraverseSpeed = 19.5f,
                TurretArmorFront = 32, TurretArmorSide = 20, TurretArmorRear = 16,
                HullArmorFront = 30, HullArmorSide = 19, HullArmorRear = 14,
                IsVisible = true
            });

            var iaL2Scout = EnsureVehicle(new Vehicle
            {
                Code = "ia_l2_scout",
                Name = "TZ-23 101F",
                FactionId = iron.Id,
                Branch = "tracked",
                Class = VehicleClass.Scout,
                Level = 2,
                PurchaseCost = 5000,
                HP = 180, ShellSpeed = 144f, ShellsCount = 149, DamageMin = 25.2f, DamageMax = 37.8f, Penetration = 90, ViewRange = 125f,
                ReloadTime = 1.25f, Accuracy = 0.88f, AimTime = 1.15f,
                Speed = 5.616f, Acceleration = 1.0f, EnginePower = 860f, TraverseAcceleration = 100f, TraverseSpeed = 39f, TurretTraverseSpeed = 24f,
                TurretArmorFront = 50, TurretArmorSide = 31, TurretArmorRear = 22,
                HullArmorFront = 46, HullArmorSide = 28, HullArmorRear = 20,
                IsVisible = true
            });

            var iaL2Guardian = EnsureVehicle(new Vehicle
            {
                Code = "ia_l2_guardian",
                Name = "TZ-42",
                FactionId = iron.Id,
                Branch = "tracked",
                Class = VehicleClass.Guardian,
                Level = 2,
                PurchaseCost = 9000,
                HP = 250, ShellSpeed = 111f, ShellsCount = 65, DamageMin = 86.4f, DamageMax = 129.6f, Penetration = 115, ViewRange = 100f,
                ReloadTime = 3.0f, Accuracy = 1.4f, AimTime = 2.35f,
                Speed = 3.54f, Acceleration = 0.775f, EnginePower = 820f, TraverseAcceleration = 77.5f, TraverseSpeed = 33f, TurretTraverseSpeed = 18.75f,
                TurretArmorFront = 76, TurretArmorSide = 50, TurretArmorRear = 34,
                HullArmorFront = 67, HullArmorSide = 48, HullArmorRear = 32,
                IsVisible = true
            });

            var iaL2Colossus = EnsureVehicle(new Vehicle
            {
                Code = "ia_l2_colossus",
                Name = "YUG Titan",
                FactionId = iron.Id,
                Branch = "tracked",
                Class = VehicleClass.Colossus,
                Level = 2,
                PurchaseCost = 15000,
                HP = 340, ShellSpeed = 90f, ShellsCount = 50, DamageMin = 115.2f, DamageMax = 172.8f, Penetration = 135, ViewRange = 85f,
                ReloadTime = 4.0f, Accuracy = 2.0f, AimTime = 3.3f,
                Speed = 2.82f, Acceleration = 0.6f, EnginePower = 940f, TraverseAcceleration = 60f, TraverseSpeed = 27f, TurretTraverseSpeed = 11.25f,
                TurretArmorFront = 116, TurretArmorSide = 72, TurretArmorRear = 50,
                HullArmorFront = 102, HullArmorSide = 66, HullArmorRear = 46,
                IsVisible = true
            });

            var iaL3ScoutWraith = EnsureVehicle(new Vehicle
            {
                Code = "ia_l3_scout_wraith",
                Name = "R30-Halberd",
                FactionId = iron.Id,
                Branch = "tracked",
                Class = VehicleClass.Scout,
                Level = 3,
                PurchaseCost = 22000,
                HP = 230, ShellSpeed = 150f, ShellsCount = 180, DamageMin = 38.016f, DamageMax = 57.024f, Penetration = 108, ViewRange = 140f,
                ReloadTime = 1.1f, Accuracy = 0.82f, AimTime = 0.9f,
                Speed = 5.95f, Acceleration = 1.0f, EnginePower = 930f, TraverseAcceleration = 100f, TraverseSpeed = 42f, TurretTraverseSpeed = 25f,
                TurretArmorFront = 62, TurretArmorSide = 38, TurretArmorRear = 26,
                HullArmorFront = 56, HullArmorSide = 35, HullArmorRear = 24,
                IsVisible = false
            });

            var iaL3GuardianWarder = EnsureVehicle(new Vehicle
            {
                Code = "ia_l3_guardian_warder",
                Name = "BR-174 Deadlock",
                FactionId = iron.Id,
                Branch = "tracked",
                Class = VehicleClass.Guardian,
                Level = 3,
                PurchaseCost = 24000,
                HP = 330, ShellSpeed = 120f, ShellsCount = 100, DamageMin = 100.8f, DamageMax = 151.2f, Penetration = 132, ViewRange = 112f,
                ReloadTime = 2.8f, Accuracy = 1.35f, AimTime = 2.0f,
                Speed = 3.75f, Acceleration = 0.85f, EnginePower = 1060f, TraverseAcceleration = 85f, TraverseSpeed = 35f, TurretTraverseSpeed = 21f,
                TurretArmorFront = 92, TurretArmorSide = 59, TurretArmorRear = 40,
                HullArmorFront = 84, HullArmorSide = 55, HullArmorRear = 38,
                IsVisible = false
            });

            var iaL3GuardianRampart = EnsureVehicle(new Vehicle
            {
                Code = "ia_l3_guardian_rampart",
                Name = "BR-160",
                FactionId = iron.Id,
                Branch = "tracked",
                Class = VehicleClass.Guardian,
                Level = 3,
                PurchaseCost = 27000,
                HP = 360, ShellSpeed = 114f, ShellsCount = 90, DamageMin = 108f, DamageMax = 162f, Penetration = 136, ViewRange = 108f,
                ReloadTime = 3.0f, Accuracy = 1.45f, AimTime = 2.2f,
                Speed = 3.45f, Acceleration = 0.8f, EnginePower = 1020f, TraverseAcceleration = 80f, TraverseSpeed = 33.5f, TurretTraverseSpeed = 20f,
                TurretArmorFront = 103, TurretArmorSide = 66, TurretArmorRear = 45,
                HullArmorFront = 94, HullArmorSide = 61, HullArmorRear = 41,
                IsVisible = false
            });

            var iaL3ColossusMastodon = EnsureVehicle(new Vehicle
            {
                Code = "ia_l3_colossus_mastodon",
                Name = "R42-Glaive 7B",
                FactionId = iron.Id,
                Branch = "tracked",
                Class = VehicleClass.Colossus,
                Level = 3,
                PurchaseCost = 36000,
                HP = 440, ShellSpeed = 100f, ShellsCount = 60, DamageMin = 162f, DamageMax = 243f, Penetration = 148, ViewRange = 92f,
                ReloadTime = 4.5f, Accuracy = 1.85f, AimTime = 3.3f,
                Speed = 2.95f, Acceleration = 0.65f, EnginePower = 1180f, TraverseAcceleration = 65f, TraverseSpeed = 28f, TurretTraverseSpeed = 12.2f,
                TurretArmorFront = 136, TurretArmorSide = 86, TurretArmorRear = 59,
                HullArmorFront = 123, HullArmorSide = 80, HullArmorRear = 55,
                IsVisible = false
            });

            var iaL4GuardianCitadel = EnsureVehicle(new Vehicle
            {
                Code = "ia_l4_guardian_citadel",
                Name = "Blackline-09 Mk. II",
                FactionId = iron.Id,
                Branch = "tracked",
                Class = VehicleClass.Guardian,
                Level = 4,
                PurchaseCost = 44000,
                HP = 455, ShellSpeed = 132f, ShellsCount = 105, DamageMin = 126f, DamageMax = 189f, Penetration = 150, ViewRange = 120f,
                ReloadTime = 2.8f, Accuracy = 1.28f, AimTime = 1.8f,
                Speed = 3.6f, Acceleration = 0.825f, EnginePower = 1160f, TraverseAcceleration = 82.5f, TraverseSpeed = 35f, TurretTraverseSpeed = 20.5f,
                TurretArmorFront = 124, TurretArmorSide = 79, TurretArmorRear = 54,
                HullArmorFront = 113, HullArmorSide = 73, HullArmorRear = 50,
                IsVisible = false
            });

            var iaL4ColossusDreadnought = EnsureVehicle(new Vehicle
            {
                Code = "ia_l4_colossus_dreadnought",
                Name = "V-88/R Northstar",
                FactionId = iron.Id,
                Branch = "tracked",
                Class = VehicleClass.Colossus,
                Level = 4,
                PurchaseCost = 56000,
                HP = 565, ShellSpeed = 96f, ShellsCount = 55, DamageMin = 207.36f, DamageMax = 311.04f, Penetration = 165, ViewRange = 96f,
                ReloadTime = 4.8f, Accuracy = 1.9f, AimTime = 3.6f,
                Speed = 2.6f, Acceleration = 0.55f, EnginePower = 1320f, TraverseAcceleration = 55f, TraverseSpeed = 25.5f, TurretTraverseSpeed = 10.5f,
                TurretArmorFront = 168, TurretArmorSide = 107, TurretArmorRear = 73,
                HullArmorFront = 151, HullArmorSide = 98, HullArmorRear = 67,
                IsVisible = false
            });

            SyncLinks(
                new[] { iaStarter, iaL2Scout, iaL2Guardian, iaL2Colossus, iaL3ScoutWraith, iaL3GuardianWarder, iaL3GuardianRampart, iaL3ColossusMastodon, iaL4GuardianCitadel, iaL4ColossusDreadnought },
                (iaStarter, iaL2Scout, 400),
                (iaL2Scout, iaL2Guardian, 700),
                (iaL2Guardian, iaL2Colossus, 1000),
                (iaStarter, iaL3GuardianRampart, 2600),
                (iaL3GuardianRampart, iaL3GuardianWarder, 2200),
                (iaL3GuardianWarder, iaL3ScoutWraith, 1800),
                (iaL3GuardianRampart, iaL3ColossusMastodon, 3200),
                (iaL3ColossusMastodon, iaL4ColossusDreadnought, 6200),
                (iaL4ColossusDreadnought, iaL4GuardianCitadel, 5000));

            // Nova Syndicate (biped) — L1 + три L2
            var nvStarter = EnsureVehicle(new Vehicle
            {
                Code = "nv_l1_starter",
                Name = "N-16 Whitecap",
                FactionId = nova.Id,
                Branch = "biped",
                Class = VehicleClass.Scout,
                Level = 1,
                PurchaseCost = 0,

                HP = 100, ShellSpeed = 135f, ShellsCount = 118, DamageMin = 38.4f, DamageMax = 57.6f, Penetration = 102, ViewRange = 110f,
                ReloadTime = 1.6f, Accuracy = 1.02f, AimTime = 1.45f,
                Speed = 1.62f, Acceleration = 0.375f, EnginePower = 1360f, TraverseAcceleration = 37.5f, TraverseSpeed = 62f, TurretTraverseSpeed = 56.95f,
                TurretArmorFront = 30, TurretArmorSide = 19, TurretArmorRear = 14,
                HullArmorFront = 29, HullArmorSide = 18, HullArmorRear = 13,
                IsVisible = true
            });

            var nvL2Scout = EnsureVehicle(new Vehicle
            {
                Code = "nv_l2_scout",
                Name = "N-17",
                FactionId = nova.Id,
                Branch = "biped",
                Class = VehicleClass.Scout,
                Level = 2,
                PurchaseCost = 5000,
                HP = 180, ShellSpeed = 150f, ShellsCount = 185, DamageMin = 28.8f, DamageMax = 43.2f, Penetration = 96, ViewRange = 130f,
                ReloadTime = 1.0f, Accuracy = 0.8f, AimTime = 0.95f,
                Speed = 1.86f, Acceleration = 0.4f, EnginePower = 1420f, TraverseAcceleration = 40f, TraverseSpeed = 72f, TurretTraverseSpeed = 68f,
                TurretArmorFront = 48, TurretArmorSide = 30, TurretArmorRear = 21,
                HullArmorFront = 43, HullArmorSide = 27, HullArmorRear = 19,
                IsVisible = true
            });

            var nvL2Guardian = EnsureVehicle(new Vehicle
            {
                Code = "nv_l2_guardian",
                Name = "ARGUS-7B",
                FactionId = nova.Id,
                Branch = "biped",
                Class = VehicleClass.Guardian,
                Level = 2,
                PurchaseCost = 9000,
                HP = 250, ShellSpeed = 120f, ShellsCount = 75, DamageMin = 74.4f, DamageMax = 112.8f, Penetration = 119, ViewRange = 105f,
                ReloadTime = 2.6f, Accuracy = 1.26f, AimTime = 2.05f,
                Speed = 1.56f, Acceleration = 0.325f, EnginePower = 1460f, TraverseAcceleration = 32.5f, TraverseSpeed = 66f, TurretTraverseSpeed = 60.35f,
                TurretArmorFront = 74, TurretArmorSide = 48, TurretArmorRear = 32,
                HullArmorFront = 67, HullArmorSide = 45, HullArmorRear = 30,
                IsVisible = true
            });

            var nvL2Colossus = EnsureVehicle(new Vehicle
            {
                Code = "nv_l2_colossus",
                Name = "Cormorant 8C",
                FactionId = nova.Id,
                Branch = "biped",
                Class = VehicleClass.Colossus,
                Level = 2,
                PurchaseCost = 15000,
                HP = 335, ShellSpeed = 99f, ShellsCount = 57, DamageMin = 100.8f, DamageMax = 151.2f, Penetration = 132, ViewRange = 90f,
                ReloadTime = 3.5f, Accuracy = 1.81f, AimTime = 2.9f,
                Speed = 1.26f, Acceleration = 0.275f, EnginePower = 1520f, TraverseAcceleration = 27.5f, TraverseSpeed = 60f, TurretTraverseSpeed = 51.85f,
                TurretArmorFront = 106, TurretArmorSide = 70, TurretArmorRear = 48,
                HullArmorFront = 94, HullArmorSide = 64, HullArmorRear = 45,
                IsVisible = true
            });

            var nvL3ScoutRift = EnsureVehicle(new Vehicle
            {
                Code = "nv_l3_scout_rift",
                Name = "VX-44/2",
                FactionId = nova.Id,
                Branch = "biped",
                Class = VehicleClass.Scout,
                Level = 3,
                PurchaseCost = 22000,
                HP = 225, ShellSpeed = 150f, ShellsCount = 215, DamageMin = 36f, DamageMax = 54f, Penetration = 108, ViewRange = 142f,
                ReloadTime = 1.0f, Accuracy = 0.78f, AimTime = 0.82f,
                Speed = 1.98f, Acceleration = 0.425f, EnginePower = 1500f, TraverseAcceleration = 42.5f, TraverseSpeed = 76f, TurretTraverseSpeed = 72f,
                TurretArmorFront = 58, TurretArmorSide = 37, TurretArmorRear = 25,
                HullArmorFront = 53, HullArmorSide = 34, HullArmorRear = 23,
                IsVisible = false
            });

            var nvL3ScoutSpecter = EnsureVehicle(new Vehicle
            {
                Code = "nv_l3_scout_specter",
                Name = "ZR-22 Knifepoint",
                FactionId = nova.Id,
                Branch = "biped",
                Class = VehicleClass.Scout,
                Level = 3,
                PurchaseCost = 25000,
                HP = 220, ShellSpeed = 150f, ShellsCount = 230, DamageMin = 30.6f, DamageMax = 45.9f, Penetration = 106, ViewRange = 145f,
                ReloadTime = 0.85f, Accuracy = 0.77f, AimTime = 0.78f,
                Speed = 2.1f, Acceleration = 0.45f, EnginePower = 1510f, TraverseAcceleration = 45f, TraverseSpeed = 78f, TurretTraverseSpeed = 74f,
                TurretArmorFront = 55, TurretArmorSide = 35, TurretArmorRear = 24,
                HullArmorFront = 50, HullArmorSide = 32, HullArmorRear = 22,
                IsVisible = false
            });

            var nvL4ScoutHorizon = EnsureVehicle(new Vehicle
            {
                Code = "nv_l4_scout_horizon",
                Name = "77-V Seeker",
                FactionId = nova.Id,
                Branch = "biped",
                Class = VehicleClass.Scout,
                Level = 4,
                PurchaseCost = 47000,
                HP = 280, ShellSpeed = 150f, ShellsCount = 245, DamageMin = 43.2f, DamageMax = 64.8f, Penetration = 116, ViewRange = 158f,
                ReloadTime = 1.0f, Accuracy = 0.74f, AimTime = 0.7f,
                Speed = 2.28f, Acceleration = 0.475f, EnginePower = 1560f, TraverseAcceleration = 47.5f, TraverseSpeed = 82f, TurretTraverseSpeed = 78f,
                TurretArmorFront = 70, TurretArmorSide = 44, TurretArmorRear = 30,
                HullArmorFront = 64, HullArmorSide = 41, HullArmorRear = 28,
                IsVisible = false
            });

            var nvL3GuardianPrism = EnsureVehicle(new Vehicle
            {
                Code = "nv_l3_guardian_prism",
                Name = "P-8 Null Compass",
                FactionId = nova.Id,
                Branch = "biped",
                Class = VehicleClass.Guardian,
                Level = 3,
                PurchaseCost = 26000,
                HP = 330, ShellSpeed = 132f, ShellsCount = 100, DamageMin = 86.4f, DamageMax = 129.6f, Penetration = 132, ViewRange = 115f,
                ReloadTime = 2.4f, Accuracy = 1.1f, AimTime = 1.7f,
                Speed = 1.74f, Acceleration = 0.36f, EnginePower = 1540f, TraverseAcceleration = 36f, TraverseSpeed = 70f, TurretTraverseSpeed = 64f,
                TurretArmorFront = 94, TurretArmorSide = 60, TurretArmorRear = 41,
                HullArmorFront = 85, HullArmorSide = 56, HullArmorRear = 38,
                IsVisible = false
            });

            var nvL3ColossusHarrow = EnsureVehicle(new Vehicle
            {
                Code = "nv_l3_colossus_harrow",
                Name = "S-404 Firebreak",
                FactionId = nova.Id,
                Branch = "biped",
                Class = VehicleClass.Colossus,
                Level = 3,
                PurchaseCost = 35000,
                HP = 435, ShellSpeed = 105f, ShellsCount = 65, DamageMin = 115.2f, DamageMax = 172.8f, Penetration = 145, ViewRange = 94f,
                ReloadTime = 3.2f, Accuracy = 1.65f, AimTime = 2.5f,
                Speed = 1.45f, Acceleration = 0.3f, EnginePower = 1600f, TraverseAcceleration = 30f, TraverseSpeed = 56f, TurretTraverseSpeed = 48f,
                TurretArmorFront = 125, TurretArmorSide = 81, TurretArmorRear = 55,
                HullArmorFront = 113, HullArmorSide = 73, HullArmorRear = 50,
                IsVisible = false
            });

            var nvL3ColossusOblivion = EnsureVehicle(new Vehicle
            {
                Code = "nv_l3_colossus_oblivion",
                Name = "A-93 Graveward",
                FactionId = nova.Id,
                Branch = "biped",
                Class = VehicleClass.Colossus,
                Level = 3,
                PurchaseCost = 39000,
                HP = 475, ShellSpeed = 102f, ShellsCount = 58, DamageMin = 138.24f, DamageMax = 207.36f, Penetration = 152, ViewRange = 90f,
                ReloadTime = 3.6f, Accuracy = 1.85f, AimTime = 2.8f,
                Speed = 1.34f, Acceleration = 0.285f, EnginePower = 1630f, TraverseAcceleration = 28.5f, TraverseSpeed = 53f, TurretTraverseSpeed = 46f,
                TurretArmorFront = 140, TurretArmorSide = 90, TurretArmorRear = 62,
                HullArmorFront = 127, HullArmorSide = 82, HullArmorRear = 56,
                IsVisible = false
            });

            var nvL4ColossusEclipse = EnsureVehicle(new Vehicle
            {
                Code = "nv_l4_colossus_eclipse",
                Name = "M-03 Sundown",
                FactionId = nova.Id,
                Branch = "biped",
                Class = VehicleClass.Colossus,
                Level = 4,
                PurchaseCost = 60000,
                HP = 590, ShellSpeed = 96f, ShellsCount = 54, DamageMin = 164.16f, DamageMax = 246.24f, Penetration = 168, ViewRange = 94f,
                ReloadTime = 3.8f, Accuracy = 1.85f, AimTime = 3.0f,
                Speed = 1.25f, Acceleration = 0.27f, EnginePower = 1700f, TraverseAcceleration = 27f, TraverseSpeed = 51f, TurretTraverseSpeed = 45f,
                TurretArmorFront = 173, TurretArmorSide = 111, TurretArmorRear = 76,
                HullArmorFront = 156, HullArmorSide = 102, HullArmorRear = 70,
                IsVisible = false
            });

            SyncLinks(
                new[] { nvStarter, nvL2Scout, nvL2Guardian, nvL2Colossus, nvL3ScoutRift, nvL3ScoutSpecter, nvL4ScoutHorizon, nvL3GuardianPrism, nvL3ColossusHarrow, nvL3ColossusOblivion, nvL4ColossusEclipse },
                (nvStarter, nvL2Scout, 400),
                (nvL2Scout, nvL2Guardian, 700),
                (nvL2Guardian, nvL2Colossus, 1000),
                (nvL2Colossus, nvL3ScoutSpecter, 2200),
                (nvStarter, nvL3GuardianPrism, 2400),
                (nvL3GuardianPrism, nvL3ScoutRift, 1700),
                (nvL3ScoutRift, nvL4ColossusEclipse, 6500),
                (nvL3ScoutRift, nvL4ScoutHorizon, 4800),
                (nvStarter, nvL3ColossusOblivion, 3600),
                (nvL3ColossusOblivion, nvL3ColossusHarrow, 3000));

            // --- Players ---
            if (includeDevelopmentFixtures && !db.Players.Any())
            {
                var user = new Player
                {
                    Username = "testuser",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("test123"),
                    IsAdmin = false,
                    Mmr = 1000,
                    FreeXp = 500,
                    Bolts = 1000,
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
            if (includeDevelopmentFixtures && !db.Matches.Any())
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
