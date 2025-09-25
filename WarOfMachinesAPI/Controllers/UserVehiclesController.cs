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

        // GET /user-vehicles/me
        // Повертає техніку користувача з позначкою активної
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
                    IsActive = x.IsActive
                })
                .ToList();

            return Ok(list);
        }

        // PUT /user-vehicles/me/active/{vehicleId}
        // Робить зазначений vehicleId активним у поточного користувача
        [HttpPut("me/active/{vehicleId:int}")]
        public IActionResult SetActive(int vehicleId)
        {
            int uid = CurrentUserId();

            var owned = _db.UserVehicles
                .Where(x => x.UserId == uid)
                .ToList();

            var target = owned.FirstOrDefault(x => x.VehicleId == vehicleId);
            if (target == null)
            {
                return NotFound("User does not own this vehicle.");
            }

            foreach (var uv in owned)
            {
                uv.IsActive = uv.VehicleId == vehicleId;
            }

            _db.SaveChanges();
            return Ok(new { ok = true, activeVehicleId = vehicleId });
        }

        // POST /user-vehicles/me/add-by-code/{code}
        // Додає техніку користувачу за кодом з каталогу (демо-онбординг/дев тул)
        [HttpPost("me/add-by-code/{code}")]
        public IActionResult AddByCode(string code)
        {
            int uid = CurrentUserId();

            var vehicle = _db.Vehicles.FirstOrDefault(v => v.Code == code);
            if (vehicle == null)
            {
                return NotFound("Vehicle code not found.");
            }

            bool already = _db.UserVehicles.Any(x => x.UserId == uid && x.VehicleId == vehicle.Id);
            if (already)
            {
                return Conflict("Vehicle already owned.");
            }

            var uv = new UserVehicle
            {
                UserId = uid,
                VehicleId = vehicle.Id,
                IsActive = false
            };

            _db.UserVehicles.Add(uv);
            _db.SaveChanges();

            return Ok(new { ok = true, userVehicleId = uv.Id, vehicleId = uv.VehicleId });
        }

        // DELETE /user-vehicles/me/{vehicleId}
        // Видаляє техніку в користувача (лише якщо це не активна) — для чистоти демо
        [HttpDelete("me/{vehicleId:int}")]
        public IActionResult Remove(int vehicleId)
        {
            int uid = CurrentUserId();

            var uv = _db.UserVehicles.FirstOrDefault(x => x.UserId == uid && x.VehicleId == vehicleId);
            if (uv == null)
            {
                return NotFound();
            }

            if (uv.IsActive)
            {
                return BadRequest("Cannot remove an active vehicle.");
            }

            _db.UserVehicles.Remove(uv);
            _db.SaveChanges();

            return Ok(new { ok = true });
        }
    }

    // DTO
    public class UserVehicleDto
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string VehicleCode { get; set; } = string.Empty;
        public string VehicleName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
