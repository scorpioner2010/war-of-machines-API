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

        // GET /players/me
        [HttpGet("me")]
        public IActionResult GetMyProfile()
        {
            int uid = CurrentUserId();

            var user = _db.Players
                .Include(p => p.UserVehicles)
                    .ThenInclude(uv => uv.Vehicle)
                .FirstOrDefault(p => p.Id == uid);

            if (user == null)
                return NotFound();

            var dto = new PlayerProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                IsAdmin = user.IsAdmin,
                Mmr = user.Mmr,
                Bolts = user.Bolts,
                Adamant = user.Adamant,
                FreeXp = user.FreeXp,
                ActiveVehicleId = user.UserVehicles.FirstOrDefault(v => v.IsActive)?.VehicleId ?? 0,
                ActiveVehicleCode = user.UserVehicles.FirstOrDefault(v => v.IsActive)?.Vehicle?.Code ?? "",
                ActiveVehicleName = user.UserVehicles.FirstOrDefault(v => v.IsActive)?.Vehicle?.Name ?? "",
                OwnedVehicles = user.UserVehicles.Select(v => new OwnedVehicleDto
                {
                    VehicleId = v.VehicleId,
                    Code = v.Vehicle != null ? v.Vehicle.Code : "",
                    Name = v.Vehicle != null ? v.Vehicle.Name : "",
                    IsActive = v.IsActive
                }).ToList()
            };

            return Ok(dto);
        }
    }

    // --- DTO ---

    public class PlayerProfileDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }

        public int Mmr { get; set; }
        public int Bolts { get; set; }
        public int Adamant { get; set; }
        public int FreeXp { get; set; }

        public int ActiveVehicleId { get; set; }
        public string ActiveVehicleCode { get; set; } = string.Empty;
        public string ActiveVehicleName { get; set; } = string.Empty;

        public List<OwnedVehicleDto> OwnedVehicles { get; set; } = new();
    }

    public class OwnedVehicleDto
    {
        public int VehicleId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
