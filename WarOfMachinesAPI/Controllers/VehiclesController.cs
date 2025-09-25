using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarOfMachines.Data;

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

        // GET /vehicles?faction=iron_alliance&branch=tracked
        // faction = код фракції (Factions.Code), branch = "tracked" | "biped"
        [HttpGet]
        public IActionResult GetAll([FromQuery] string? faction = null, [FromQuery] string? branch = null)
        {
            var q = _db.Vehicles
                .Include(v => v.Faction)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(faction))
            {
                string fc = faction.Trim();
                q = q.Where(v => v.Faction != null && v.Faction.Code == fc);
            }

            if (!string.IsNullOrWhiteSpace(branch))
            {
                string br = branch.Trim().ToLowerInvariant();
                q = q.Where(v => v.Branch.ToLower() == br);
            }

            var items = q
                .Select(v => new VehicleDto
                {
                    Id = v.Id,
                    Code = v.Code,
                    Name = v.Name,
                    Branch = v.Branch,
                    FactionCode = v.Faction != null ? v.Faction.Code : string.Empty,
                    FactionName = v.Faction != null ? v.Faction.Name : string.Empty,
                    Stats = v.Stats.RootElement.Clone()
                })
                .ToList();

            return Ok(items);
        }

        // GET /vehicles/{id:int}
        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            var v = _db.Vehicles
                .Include(x => x.Faction)
                .FirstOrDefault(x => x.Id == id);
            if (v == null) return NotFound();

            return Ok(new VehicleDto
            {
                Id = v.Id,
                Code = v.Code,
                Name = v.Name,
                Branch = v.Branch,
                FactionCode = v.Faction != null ? v.Faction.Code : string.Empty,
                FactionName = v.Faction != null ? v.Faction.Name : string.Empty,
                Stats = v.Stats.RootElement.Clone()
            });
        }

        // GET /vehicles/by-code/{code}
        [HttpGet("by-code/{code}")]
        public IActionResult GetByCode(string code)
        {
            var v = _db.Vehicles
                .Include(x => x.Faction)
                .FirstOrDefault(x => x.Code == code);
            if (v == null) return NotFound();

            return Ok(new VehicleDto
            {
                Id = v.Id,
                Code = v.Code,
                Name = v.Name,
                Branch = v.Branch,
                FactionCode = v.Faction != null ? v.Faction.Code : string.Empty,
                FactionName = v.Faction != null ? v.Faction.Name : string.Empty,
                Stats = v.Stats.RootElement.Clone()
            });
        }
    }

    public class VehicleDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public string Branch { get; set; } = string.Empty; // "tracked" | "biped"
        public string FactionCode { get; set; } = string.Empty;
        public string FactionName { get; set; } = string.Empty;

        public JsonElement Stats { get; set; }
    }
}
