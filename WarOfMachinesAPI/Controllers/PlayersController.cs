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
            int userId = CurrentUserId();

            var user = _db.Players.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return NotFound();

            var owned = _db.UserVehicles
                .Where(x => x.UserId == userId)
                .Include(x => x.Vehicle)
                .ToList();

            var active = owned.FirstOrDefault(x => x.IsActive);

            var dto = new PlayerProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                IsAdmin = user.IsAdmin,
                Mmr = user.Mmr,
                FreeXp = user.FreeXp,
                Bolts = user.Bolts,
                Adamant = user.Adamant,
                ActiveVehicleId = active?.VehicleId,
                ActiveVehicleCode = active?.Vehicle?.Code,
                ActiveVehicleName = active?.Vehicle?.Name,
                OwnedVehicles = owned.Select(x => new OwnedVehicleDto
                {
                    VehicleId = x.VehicleId,
                    Code = x.Vehicle?.Code ?? "",
                    Name = x.Vehicle?.Name ?? "",
                    IsActive = x.IsActive,
                    Xp = x.Xp
                }).ToArray()
            };

            return Ok(dto);
        }

        [HttpPut("me/active/{vehicleId:int}")]
        public IActionResult SetActiveVehicle(int vehicleId)
        {
            int userId = CurrentUserId();

            var owned = _db.UserVehicles.Where(x => x.UserId == userId).ToList();
            var target = owned.FirstOrDefault(x => x.VehicleId == vehicleId);
            if (target == null)
                return NotFound("User does not own this vehicle.");

            foreach (var uv in owned)
                uv.IsActive = uv.VehicleId == vehicleId;

            _db.SaveChanges();
            return Ok(new { ok = true, activeVehicleId = vehicleId });
        }
    }

    public class PlayerProfileDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public int Mmr { get; set; }
        public int FreeXp { get; set; }
        public int Bolts { get; set; }
        public int Adamant { get; set; }

        public int? ActiveVehicleId { get; set; }
        public string? ActiveVehicleCode { get; set; }
        public string? ActiveVehicleName { get; set; }

        public OwnedVehicleDto[] OwnedVehicles { get; set; } = System.Array.Empty<OwnedVehicleDto>();
    }

    public class OwnedVehicleDto
    {
        public int VehicleId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int Xp { get; set; }
    }
}
