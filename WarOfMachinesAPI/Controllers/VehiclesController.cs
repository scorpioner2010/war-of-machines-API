using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using WarOfMachines.Data;
using WarOfMachines.Models;

namespace WarOfMachines.Controllers
{
    [ApiController]
    [Route("vehicles")]
    public class VehiclesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public VehiclesController(AppDbContext db)
        {
            _db = db;
        }

        // GET /vehicles
        [HttpGet]
        public IActionResult GetAll()
        {
            var items = _db.Vehicles
                .Select(v => new VehicleDto
                {
                    Id = v.Id,
                    Code = v.Code,
                    Name = v.Name,
                    Stats = v.Stats.RootElement.Clone()
                })
                .ToList();

            return Ok(items);
        }

        // GET /vehicles/{id:int}
        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            var v = _db.Vehicles.FirstOrDefault(x => x.Id == id);
            if (v == null)
            {
                return NotFound();
            }

            return Ok(new VehicleDto
            {
                Id = v.Id,
                Code = v.Code,
                Name = v.Name,
                Stats = v.Stats.RootElement.Clone()
            });
        }

        // GET /vehicles/by-code/{code}
        [HttpGet("by-code/{code}")]
        public IActionResult GetByCode(string code)
        {
            var v = _db.Vehicles.FirstOrDefault(x => x.Code == code);
            if (v == null)
            {
                return NotFound();
            }

            return Ok(new VehicleDto
            {
                Id = v.Id,
                Code = v.Code,
                Name = v.Name,
                Stats = v.Stats.RootElement.Clone()
            });
        }
    }

    // Простий DTO для серіалізації (Stats як JsonElement)
    public class VehicleDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public JsonElement Stats { get; set; }
    }
}
