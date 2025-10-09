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

                OwnedVehicles = owned.Select(v => new OwnedVehicleDto
                {
                    VehicleId = v.VehicleId,
                    Code = v.Vehicle?.Code ?? string.Empty,
                    Name = v.Vehicle?.Name ?? string.Empty,
                    IsActive = v.IsActive,
                    Xp = v.Xp
                }).ToList()
            };

            return Ok(dto);
        }

        [HttpPut("me/active/{vehicleId:int}")]
        public IActionResult SetActive(int vehicleId)
        {
            int uid = CurrentUserId();

            var owned = _db.UserVehicles.Where(x => x.UserId == uid).ToList();
            var target = owned.FirstOrDefault(x => x.VehicleId == vehicleId);
            if (target == null) return NotFound("User does not own this vehicle.");

            foreach (var uv in owned)
                uv.IsActive = (uv.VehicleId == vehicleId);

            _db.SaveChanges();
            return Ok(new { ok = true, activeVehicleId = vehicleId });
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

        public List<OwnedVehicleDto> OwnedVehicles { get; set; } = new();
    }

    public class OwnedVehicleDto
    {
        public int VehicleId { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public bool IsActive { get; set; }
        public int Xp { get; set; }
    }
}
