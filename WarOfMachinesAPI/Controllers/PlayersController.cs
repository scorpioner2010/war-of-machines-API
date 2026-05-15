using System;
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
                ActiveVehicleTurretTraverseSpeed = active?.Vehicle?.TurretTraverseSpeed ?? 0f,

                OwnedVehicles = owned.Select(v => new OwnedVehicleDto
                {
                    VehicleId = v.VehicleId,
                    Code = v.Vehicle?.Code ?? string.Empty,
                    Name = v.Vehicle?.Name ?? string.Empty,
                    ShellSpeed = v.Vehicle?.ShellSpeed ?? 0f,
                    TurretTraverseSpeed = v.Vehicle?.TurretTraverseSpeed ?? 0f,
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
        public float ActiveVehicleTurretTraverseSpeed { get; set; }

        public List<OwnedVehicleDto> OwnedVehicles { get; set; } = new();
        public List<ResearchedVehicleDto> ResearchedVehicles { get; set; } = new();
    }

    public class OwnedVehicleDto
    {
        public int VehicleId { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public float ShellSpeed { get; set; }
        public float TurretTraverseSpeed { get; set; }
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
}
