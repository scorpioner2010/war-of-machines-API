using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarOfMachines.Data;

namespace WarOfMachines.Controllers
{
    [ApiController]
    [Route("players")]
    [Authorize]
    public class PlayersController : ControllerBase
    {
        private readonly AppDbContext _db;

        public PlayersController(AppDbContext db)
        {
            _db = db;
        }

        private int CurrentUserId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.Parse(idStr);
        }

        [HttpGet("me")]
        public IActionResult GetMe()
        {
            int uid = CurrentUserId();

            var p = _db.Players.FirstOrDefault(x => x.Id == uid);
            if (p == null) return NotFound();

            var owned = _db.UserVehicles
                .Where(u => u.UserId == uid)                 // ВАЖЛИВО: саме UserId
                .Include(u => u.Vehicle)
                .AsNoTracking()
                .ToList();

            var active = owned.FirstOrDefault(v => v.IsActive);
            var researchedVehicles = _db.UserVehicleResearches
                .Where(r => r.UserId == uid)
                .Include(r => r.Vehicle)
                .AsNoTracking()
                .ToList();
            var researchedIds = researchedVehicles
                .Select(r => r.VehicleId)
                .ToHashSet();

            var dto = new PlayerProfileDto
            {
                Id = p.Id,
                Username = p.Username,
                IsAdmin = p.IsAdmin,
                Mmr = p.Mmr,
                Bolts = p.Bolts,
                Adamant = p.Adamant,
                FreeXp = p.FreeXp,

                ActiveVehicleId = active?.VehicleId ?? 0,
                ActiveVehicleCode = active?.Vehicle?.Code ?? string.Empty,
                ActiveVehicleName = active?.Vehicle?.Name ?? string.Empty,
                ActiveVehicleShellSpeed = active?.Vehicle?.ShellSpeed ?? 0f,
                ActiveVehicleTraverseAcceleration = active?.Vehicle?.TraverseAcceleration ?? 0f,
                ActiveVehicleTurretTraverseSpeed = active?.Vehicle?.TurretTraverseSpeed ?? 0f,
                ActiveVehicleViewRange = active?.Vehicle?.ViewRange ?? 0f,

                OwnedVehicles = owned.Select(v => new OwnedVehicleDto
                {
                    VehicleId = v.VehicleId,
                    Code = v.Vehicle?.Code ?? string.Empty,
                    Name = v.Vehicle?.Name ?? string.Empty,
                    ShellSpeed = v.Vehicle?.ShellSpeed ?? 0f,
                    TraverseAcceleration = v.Vehicle?.TraverseAcceleration ?? 0f,
                    TurretTraverseSpeed = v.Vehicle?.TurretTraverseSpeed ?? 0f,
                    ViewRange = v.Vehicle?.ViewRange ?? 0f,
                    IsActive = v.IsActive,
                    Xp = v.Xp,
                    IsResearched = researchedIds.Contains(v.VehicleId)
                }).ToList(),

                ResearchedVehicles = researchedVehicles.Select(r => new ResearchedVehicleDto
                {
                    VehicleId = r.VehicleId,
                    Code = r.Vehicle?.Code ?? string.Empty,
                    Name = r.Vehicle?.Name ?? string.Empty
                }).ToList()
            };

            return Ok(dto);
        }

        [HttpPut("me/active/{vehicleId:int}")]
        public IActionResult SetActive(int vehicleId)
        {
            int uid = CurrentUserId();

            var strategy = _db.Database.CreateExecutionStrategy();
            return strategy.Execute<IActionResult>(() =>
            {
                using var tx = _db.Database.BeginTransaction();

                var owned = _db.UserVehicles.Where(x => x.UserId == uid).ToList();
                var target = owned.FirstOrDefault(x => x.VehicleId == vehicleId);
                if (target == null)
                {
                    tx.Rollback();
                    return NotFound("User does not own this vehicle.");
                }

                if (target.IsActive)
                {
                    tx.Commit();
                    return Ok(new { ok = true, activeVehicleId = vehicleId });
                }

                var currentActive = owned.FirstOrDefault(x => x.IsActive);
                if (currentActive != null)
                {
                    currentActive.IsActive = false;
                    _db.SaveChanges();
                }

                target.IsActive = true;
                _db.SaveChanges();

                tx.Commit();
                return Ok(new { ok = true, activeVehicleId = vehicleId });
            });
        }

        [HttpGet("me/statistics")]
        public IActionResult GetMyStatistics()
        {
            int userId = CurrentUserId();
            var player = _db.Players
                .AsNoTracking()
                .FirstOrDefault(x => x.Id == userId);
            if (player == null)
            {
                return NotFound();
            }

            List<PlayerStatisticsRecord> records = _db.MatchParticipants
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .Select(x => new PlayerStatisticsRecord
                {
                    VehicleId = x.VehicleId,
                    VehicleCode = x.Vehicle != null ? x.Vehicle.Code : string.Empty,
                    VehicleName = x.Vehicle != null ? x.Vehicle.Name : string.Empty,
                    VehicleTier = x.Vehicle != null ? x.Vehicle.Level : 0,
                    VehicleFactionCode = x.Vehicle != null && x.Vehicle.Faction != null
                        ? x.Vehicle.Faction.Code
                        : string.Empty,
                    VehicleBranch = x.Vehicle != null ? x.Vehicle.Branch : string.Empty,
                    Result = x.Result,
                    Kills = x.Kills,
                    Damage = x.Damage,
                    XpEarned = x.XpEarned,
                    BoltsEarned = x.BoltsEarned,
                    FreeXpEarned = x.FreeXpEarned,
                    MmrDelta = x.MmrDelta,
                    BattleAt = x.Match == null
                        ? null
                        : x.Match.EndedAt ?? x.Match.StartedAt
                })
                .ToList();

            Dictionary<int, OwnedVehicleStatistics> ownedVehicles = _db.UserVehicles
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .Select(x => new OwnedVehicleStatistics
                {
                    VehicleId = x.VehicleId,
                    CurrentXp = x.Xp,
                    IsActive = x.IsActive
                })
                .ToDictionary(x => x.VehicleId, x => x);

            var statistics = new PlayerStatisticsDto
            {
                Username = player.Username,
                Mmr = player.Mmr,
                Bolts = player.Bolts,
                Adamant = player.Adamant,
                FreeXp = player.FreeXp
            };
            var vehicleStatistics = new Dictionary<int, VehicleStatisticsDto>();

            foreach (PlayerStatisticsRecord record in records)
            {
                statistics.Battles++;
                statistics.Kills += Math.Max(0, record.Kills);
                statistics.Damage += Math.Max(0, record.Damage);
                statistics.XpEarned += Math.Max(0, record.XpEarned);
                statistics.BoltsEarned += Math.Max(0, record.BoltsEarned);
                statistics.FreeXpEarned += Math.Max(0, record.FreeXpEarned);
                statistics.MmrDelta += record.MmrDelta;

                if (IsResult(record.Result, "win"))
                {
                    statistics.Wins++;
                }
                else if (IsResult(record.Result, "draw"))
                {
                    statistics.Draws++;
                }
                else
                {
                    statistics.Losses++;
                }

                if (record.BattleAt.HasValue && (!statistics.LastBattleAt.HasValue || record.BattleAt > statistics.LastBattleAt))
                {
                    statistics.LastBattleAt = record.BattleAt;
                }

                if (!vehicleStatistics.TryGetValue(record.VehicleId, out VehicleStatisticsDto? vehicle) || vehicle == null)
                {
                    vehicle = new VehicleStatisticsDto
                    {
                        VehicleId = record.VehicleId,
                        Code = record.VehicleCode,
                        Name = string.IsNullOrWhiteSpace(record.VehicleName) ? "Unknown vehicle" : record.VehicleName,
                        Tier = record.VehicleTier,
                        FactionCode = record.VehicleFactionCode,
                        Branch = record.VehicleBranch
                    };
                    vehicleStatistics.Add(record.VehicleId, vehicle);
                }

                vehicle.Battles++;
                vehicle.Kills += Math.Max(0, record.Kills);
                vehicle.Damage += Math.Max(0, record.Damage);
                vehicle.XpEarned += Math.Max(0, record.XpEarned);
                vehicle.BoltsEarned += Math.Max(0, record.BoltsEarned);
                vehicle.FreeXpEarned += Math.Max(0, record.FreeXpEarned);

                if (IsResult(record.Result, "win"))
                {
                    vehicle.Wins++;
                }
                else if (IsResult(record.Result, "draw"))
                {
                    vehicle.Draws++;
                }
                else
                {
                    vehicle.Losses++;
                }

                if (record.Kills > vehicle.BestKills)
                {
                    vehicle.BestKills = record.Kills;
                }

                if (record.Damage > vehicle.BestDamage)
                {
                    vehicle.BestDamage = record.Damage;
                }

                if (record.BattleAt.HasValue && (!vehicle.LastPlayedAt.HasValue || record.BattleAt > vehicle.LastPlayedAt))
                {
                    vehicle.LastPlayedAt = record.BattleAt;
                }
            }

            foreach (VehicleStatisticsDto vehicle in vehicleStatistics.Values)
            {
                if (ownedVehicles.TryGetValue(vehicle.VehicleId, out OwnedVehicleStatistics? owned) && owned != null)
                {
                    vehicle.IsOwned = true;
                    vehicle.IsActive = owned.IsActive;
                    vehicle.CurrentXp = owned.CurrentXp;
                }

                vehicle.WinRate = CalculatePercentage(vehicle.Wins, vehicle.Battles);
                vehicle.AverageDamage = CalculateAverage(vehicle.Damage, vehicle.Battles);
                vehicle.AverageKills = CalculateAverage(vehicle.Kills, vehicle.Battles);
            }

            statistics.WinRate = CalculatePercentage(statistics.Wins, statistics.Battles);
            statistics.AverageDamage = CalculateAverage(statistics.Damage, statistics.Battles);
            statistics.AverageKills = CalculateAverage(statistics.Kills, statistics.Battles);
            statistics.Vehicles = vehicleStatistics.Values
                .OrderByDescending(x => x.Battles)
                .ThenByDescending(x => x.WinRate)
                .ThenBy(x => x.Name)
                .ToList();

            return Ok(statistics);
        }

        private static bool IsResult(string result, string expected)
        {
            return string.Equals(result, expected, StringComparison.OrdinalIgnoreCase);
        }

        private static double CalculatePercentage(int numerator, int denominator)
        {
            return denominator > 0 ? Math.Round(numerator * 100.0 / denominator, 1) : 0.0;
        }

        private static double CalculateAverage(int total, int count)
        {
            return count > 0 ? Math.Round(total / (double)count, 1) : 0.0;
        }
    }

    internal sealed class PlayerStatisticsRecord
    {
        public int VehicleId { get; set; }
        public string VehicleCode { get; set; } = string.Empty;
        public string VehicleName { get; set; } = string.Empty;
        public int VehicleTier { get; set; }
        public string VehicleFactionCode { get; set; } = string.Empty;
        public string VehicleBranch { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        public int Kills { get; set; }
        public int Damage { get; set; }
        public int XpEarned { get; set; }
        public int BoltsEarned { get; set; }
        public int FreeXpEarned { get; set; }
        public int MmrDelta { get; set; }
        public DateTimeOffset? BattleAt { get; set; }
    }

    internal sealed class OwnedVehicleStatistics
    {
        public int VehicleId { get; set; }
        public int CurrentXp { get; set; }
        public bool IsActive { get; set; }
    }

    public class PlayerProfileDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public bool IsAdmin { get; set; }
        public int Mmr { get; set; }
        public int Bolts { get; set; }
        public int Adamant { get; set; }
        public int FreeXp { get; set; }

        public int ActiveVehicleId { get; set; }
        public string ActiveVehicleCode { get; set; } = "";
        public string ActiveVehicleName { get; set; } = "";
        public float ActiveVehicleShellSpeed { get; set; }
        public float ActiveVehicleTraverseAcceleration { get; set; }
        public float ActiveVehicleTurretTraverseSpeed { get; set; }
        public float ActiveVehicleViewRange { get; set; }

        public List<OwnedVehicleDto> OwnedVehicles { get; set; } = new();
        public List<ResearchedVehicleDto> ResearchedVehicles { get; set; } = new();
    }

    public class OwnedVehicleDto
    {
        public int VehicleId { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public float ShellSpeed { get; set; }
        public float TraverseAcceleration { get; set; }
        public float TurretTraverseSpeed { get; set; }
        public float ViewRange { get; set; }
        public bool IsActive { get; set; }
        public int Xp { get; set; }
        public bool IsResearched { get; set; }
    }

    public class ResearchedVehicleDto
    {
        public int VehicleId { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
    }

    public class PlayerStatisticsDto
    {
        public string Username { get; set; } = "";
        public int Mmr { get; set; }
        public int Bolts { get; set; }
        public int Adamant { get; set; }
        public int FreeXp { get; set; }
        public int Battles { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
        public double WinRate { get; set; }
        public int Kills { get; set; }
        public int Damage { get; set; }
        public double AverageDamage { get; set; }
        public double AverageKills { get; set; }
        public int XpEarned { get; set; }
        public int BoltsEarned { get; set; }
        public int FreeXpEarned { get; set; }
        public int MmrDelta { get; set; }
        public DateTimeOffset? LastBattleAt { get; set; }
        public List<VehicleStatisticsDto> Vehicles { get; set; } = new();
    }

    public class VehicleStatisticsDto
    {
        public int VehicleId { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string FactionCode { get; set; } = "";
        public string Branch { get; set; } = "";
        public int Tier { get; set; }
        public bool IsOwned { get; set; }
        public bool IsActive { get; set; }
        public int CurrentXp { get; set; }
        public int Battles { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
        public double WinRate { get; set; }
        public int Kills { get; set; }
        public int Damage { get; set; }
        public double AverageDamage { get; set; }
        public double AverageKills { get; set; }
        public int BestKills { get; set; }
        public int BestDamage { get; set; }
        public int XpEarned { get; set; }
        public int BoltsEarned { get; set; }
        public int FreeXpEarned { get; set; }
        public DateTimeOffset? LastPlayedAt { get; set; }
    }
}
