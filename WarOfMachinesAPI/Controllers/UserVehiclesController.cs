using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarOfMachines.Data;
using WarOfMachines.Models;

namespace WarOfMachines.Controllers
{
    [ApiController]
    [Route("user-vehicles")]
    [Authorize]
    public class UserVehiclesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public UserVehiclesController(AppDbContext db)
        {
            _db = db;
        }

        private int CurrentUserId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.Parse(idStr);
        }

        // ========================================
        // GET /user-vehicles/me
        // ========================================
        [HttpGet("me")]
        public IActionResult GetMyVehicles()
        {
            int uid = CurrentUserId();

            var list = _db.UserVehicles
                .Where(x => x.UserId == uid)
                .Include(x => x.Vehicle)
                .Select(x => new UserVehicleDto
                {
                    Id = x.Id,
                    VehicleId = x.VehicleId,
                    VehicleCode = x.Vehicle != null ? x.Vehicle.Code : string.Empty,
                    VehicleName = x.Vehicle != null ? x.Vehicle.Name : string.Empty,
                    Xp = x.Xp,
                    IsActive = x.IsActive
                })
                .ToList();

            var freeXp = _db.Players
                .Where(p => p.Id == uid)
                .Select(p => p.FreeXp)
                .FirstOrDefault();

            return Ok(new
            {
                FreeXp = freeXp,
                Vehicles = list
            });
        }

        // ========================================
        // PUT /user-vehicles/me/active/{vehicleId}
        // ========================================
        [HttpPut("me/active/{vehicleId:int}")]
        public IActionResult SetActive(int vehicleId)
        {
            int uid = CurrentUserId();

            List<UserVehicle> owned = _db.UserVehicles
                .Where(x => x.UserId == uid)
                .ToList();

            UserVehicle? target = owned.FirstOrDefault(x => x.VehicleId == vehicleId);
            if (target == null)
                return NotFound("User does not own this vehicle.");

            foreach (var uv in owned)
                uv.IsActive = uv.VehicleId == vehicleId;

            _db.SaveChanges();
            return Ok(new { ok = true, activeVehicleId = vehicleId });
        }

        // ========================================
        // POST /user-vehicles/me/add-by-code/{code}
        // ========================================
        [HttpPost("me/add-by-code/{code}")]
        public IActionResult AddByCode(string code)
        {
            int uid = CurrentUserId();

            Vehicle? vehicle = _db.Vehicles.FirstOrDefault(v => v.Code == code);
            if (vehicle == null)
                return NotFound("Vehicle code not found.");

            bool already = _db.UserVehicles.Any(x => x.UserId == uid && x.VehicleId == vehicle.Id);
            if (already)
                return Conflict("Vehicle already owned.");

            UserVehicle uv = new UserVehicle
            {
                UserId = uid,
                VehicleId = vehicle.Id,
                Xp = 0,
                IsActive = false
            };

            _db.UserVehicles.Add(uv);
            _db.SaveChanges();

            return Ok(new { ok = true, userVehicleId = uv.Id, vehicleId = uv.VehicleId });
        }

        // ========================================
        // DELETE /user-vehicles/me/{vehicleId}
        // Продає техніку (з урахуванням активної)
        // ========================================
        [HttpDelete("me/{vehicleId:int}")]
        public IActionResult Remove(int vehicleId)
        {
            int uid = CurrentUserId();

            List<UserVehicle> owned = _db.UserVehicles.Where(x => x.UserId == uid).Include(x => x.Vehicle).ToList();

            if (owned.Count <= 1)
            {
                return BadRequest("Cannot sell your last remaining vehicle.");
            }

            var uv = owned.FirstOrDefault(x => x.VehicleId == vehicleId);
            if (uv == null)
            {
                return NotFound("Vehicle not found.");
            }

            // Якщо продаємо активного — активуємо інший
            if (uv.IsActive)
            {
                var replacement = owned.FirstOrDefault(x => x.VehicleId != vehicleId);
                if (replacement != null)
                {
                    replacement.IsActive = true;
                }
            }

            _db.UserVehicles.Remove(uv);
            _db.SaveChanges();

            return Ok(new
            {
                ok = true, soldVehicleId = vehicleId
            });
        }

        // ========================================
        // GET /user-vehicles/xp
        // ========================================
        [HttpGet("xp")]
        public IActionResult GetMyVehiclesXp()
        {
            int userId = CurrentUserId();

            var list = _db.UserVehicles
                .Where(x => x.UserId == userId)
                .Include(x => x.Vehicle)
                .Select(x => new
                {
                    x.VehicleId,
                    VehicleName = x.Vehicle != null ? x.Vehicle.Name : "",
                    x.Xp,
                    x.IsActive
                })
                .ToList();

            var freeXp = _db.Players.Where(p => p.Id == userId).Select(p => p.FreeXp).FirstOrDefault();

            return Ok(new
            {
                FreeXp = freeXp,
                Vehicles = list
            });
        }

        // ========================================
        // POST /user-vehicles/{vehicleId}/convert-freexp
        // ========================================
        [HttpPost("{vehicleId:int}/convert-freexp")]
        public IActionResult ConvertFreeXpToVehicle(int vehicleId, [FromBody] ConvertFreeXpRequest req)
        {
            int userId = CurrentUserId();

            if (req.Amount <= 0)
                return BadRequest("Amount must be positive.");

            var player = _db.Players.FirstOrDefault(p => p.Id == userId);
            if (player == null)
                return NotFound("Player not found.");

            if (player.FreeXp < req.Amount)
                return BadRequest("Not enough Free XP.");

            var uv = _db.UserVehicles
                .Include(x => x.Vehicle)
                .FirstOrDefault(x => x.UserId == userId && x.VehicleId == vehicleId);

            if (uv == null)
                return NotFound("Vehicle not found or not owned.");

            player.FreeXp -= req.Amount;
            uv.Xp += req.Amount;

            _db.SaveChanges();

            return Ok(new
            {
                ok = true,
                vehicleId = uv.VehicleId,
                vehicleName = uv.Vehicle?.Name,
                addedXp = req.Amount,
                newVehicleXp = uv.Xp,
                remainingFreeXp = player.FreeXp
            });
        }

        // ========================================
        // DTOs
        // ========================================
        public class UserVehicleDto
        {
            public int Id { get; set; }
            public int VehicleId { get; set; }
            public string VehicleCode { get; set; } = string.Empty;
            public string VehicleName { get; set; } = string.Empty;
            public int Xp { get; set; }
            public bool IsActive { get; set; }
        }

        public class ConvertFreeXpRequest
        {
            public int Amount { get; set; }
        }
    }
}
