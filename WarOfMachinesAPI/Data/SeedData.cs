using System;
using System.Linq;
using WarOfMachines.Models;

namespace WarOfMachines.Data
{
    public static class SeedData
    {
        public static void Initialize(AppDbContext db)
        {
            // --- Factions (фракції: у цьому лорі замість націй) ---
            var iron = db.Factions.FirstOrDefault(f => f.Code == "iron_alliance")
                       ?? db.Factions.Add(new Faction
                       {
                           Code = "iron_alliance",
                           Name = "Iron Alliance", // Залізний Альянс
                           Description = "Veteran pilots in armored warframes forged for frontal assaults." // Ветерани-пілоти у броньованих варфреймах для лобових штурмів
                       }).Entity;

            var nova = db.Factions.FirstOrDefault(f => f.Code == "nova_syndicate")
                       ?? db.Factions.Add(new Faction
                       {
                           Code = "nova_syndicate",
                           Name = "Nova Syndicate", // Нова Синдикат
                           Description = "A covert network fielding agile, high-tech combat machines." // Тіньова мережа з маневровими високотехнологічними машинами
                       }).Entity;

            db.SaveChanges();

            // --- Maps (мапи) ---
            if (!db.Maps.Any())
            {
                db.Maps.AddRange(
                    new Map { Code = "demo_map",    Name = "Demo Yard",   Description = "Training pit for rookie pilots." }, // Навчальний майданчик для новачків
                    new Map { Code = "steel_arena", Name = "Steel Arena", Description = "Circular proving grounds with scattered cover." } // Кільцева арена з розкиданими укриттями
                );
                db.SaveChanges();
            }

            // --- Vehicles (бойові машини-роботи; один пілот керує однією машиною) ---
            if (!db.Vehicles.Any())
            {
                db.Vehicles.AddRange(
                    // Iron Alliance (Залізний Альянс) — важкі, живучі, з товстою бронею
                    new Vehicle
                    {
                        Code = "starter",
                        Name = "IA Skirmisher Mk.I", // Легкий «Скермішер»
                        FactionId = iron.Id,
                        Branch = "tracked", // гусеничне шасі (робот на базі гусениць)

                        HP = 120,
                        Damage = 12,
                        Penetration = 40,

                        ReloadTime = 2.5f,
                        Accuracy = 0.85f,
                        AimTime = 1.8f,

                        Speed = 6f,
                        Acceleration = 3.5f,
                        TraverseSpeed = 35f,
                        TurretTraverseSpeed = 30f,

                        TurretArmorFront = 40, TurretArmorSide = 25, TurretArmorRear = 20,
                        HullArmorFront   = 50, HullArmorSide   = 30, HullArmorRear   = 25
                    },
                    new Vehicle
                    {
                        Code = "ia_heavy",
                        Name = "IA Bastion Mk.II", // Тяжкий «Бастіон»
                        FactionId = iron.Id,
                        Branch = "tracked",

                        HP = 320,
                        Damage = 30,
                        Penetration = 110,

                        ReloadTime = 4.5f,
                        Accuracy = 0.75f,
                        AimTime = 2.8f,

                        Speed = 2f,
                        Acceleration = 1.5f,
                        TraverseSpeed = 20f,
                        TurretTraverseSpeed = 18f,

                        TurretArmorFront = 200, TurretArmorSide = 120, TurretArmorRear = 80,
                        HullArmorFront   = 220, HullArmorSide   = 140, HullArmorRear   = 100
                    },
                    new Vehicle
                    {
                        Code = "ia_biped",
                        Name = "IA Strider", // Двоногий «Страйдер»
                        FactionId = iron.Id,
                        Branch = "biped", // двонога платформа

                        HP = 180,
                        Damage = 18,
                        Penetration = 80,

                        ReloadTime = 3.2f,
                        Accuracy = 0.82f,
                        AimTime = 2.2f,

                        Speed = 5f,
                        Acceleration = 3.0f,
                        TraverseSpeed = 32f,
                        TurretTraverseSpeed = 28f,

                        TurretArmorFront = 90, TurretArmorSide = 60, TurretArmorRear = 45,
                        HullArmorFront   = 110, HullArmorSide   = 70, HullArmorRear   = 55
                    },

                    // Nova Syndicate (Нова Синдикат) — мобільні, високотехнологічні, ставка на швидкість і точність
                    new Vehicle
                    {
                        Code = "nv_light",
                        Name = "Nova Wisp", // Легкий «Вісп»
                        FactionId = nova.Id,
                        Branch = "biped",

                        HP = 100,
                        Damage = 14,
                        Penetration = 60,

                        ReloadTime = 2.2f,
                        Accuracy = 0.86f,
                        AimTime = 1.6f,

                        Speed = 7f,
                        Acceleration = 4.0f,
                        TraverseSpeed = 38f,
                        TurretTraverseSpeed = 34f,

                        TurretArmorFront = 35, TurretArmorSide = 22, TurretArmorRear = 18,
                        HullArmorFront   = 40, HullArmorSide   = 26, HullArmorRear   = 20
                    },
                    new Vehicle
                    {
                        Code = "nv_tank",
                        Name = "Nova Bulwark", // Тяжкий «Булворк»
                        FactionId = nova.Id,
                        Branch = "tracked",

                        HP = 280,
                        Damage = 26,
                        Penetration = 100,

                        ReloadTime = 4.0f,
                        Accuracy = 0.78f,
                        AimTime = 2.6f,

                        Speed = 3f,
                        Acceleration = 1.8f,
                        TraverseSpeed = 22f,
                        TurretTraverseSpeed = 20f,

                        TurretArmorFront = 180, TurretArmorSide = 110, TurretArmorRear = 70,
                        HullArmorFront   = 200, HullArmorSide   = 120, HullArmorRear   = 85
                    },
                    new Vehicle
                    {
                        Code = "nv_assault",
                        Name = "Nova Raptor", // Штурмовий «Раптор»
                        FactionId = nova.Id,
                        Branch = "biped",

                        HP = 160,
                        Damage = 22,
                        Penetration = 85,

                        ReloadTime = 3.0f,
                        Accuracy = 0.84f,
                        AimTime = 2.0f,

                        Speed = 6f,
                        Acceleration = 3.6f,
                        TraverseSpeed = 34f,
                        TurretTraverseSpeed = 30f,

                        TurretArmorFront = 80, TurretArmorSide = 55, TurretArmorRear = 40,
                        HullArmorFront   = 95, HullArmorSide   = 65, HullArmorRear   = 48
                    }
                );
                db.SaveChanges();
            }

            // --- Players (гравець за замовчуванням; один пілот — одна машина) ---
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

            // --- Demo Match (демо-бій між пілотованими машинами) ---
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
