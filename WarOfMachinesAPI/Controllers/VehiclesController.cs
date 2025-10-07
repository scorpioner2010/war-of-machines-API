using System.Linq;
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

                    Class = v.Class.ToString(),
                    Level = v.Level,
                    PurchaseCost = v.PurchaseCost,

                    HP = v.HP,
                    Damage = v.Damage,
                    Penetration = v.Penetration,
                    ReloadTime = v.ReloadTime,
                    Accuracy = v.Accuracy,
                    AimTime = v.AimTime,
                    Speed = v.Speed,
                    Acceleration = v.Acceleration,
                    TraverseSpeed = v.TraverseSpeed,
                    TurretTraverseSpeed = v.TurretTraverseSpeed,

                    TurretArmor = $"{v.TurretArmorFront}/{v.TurretArmorSide}/{v.TurretArmorRear}",
                    HullArmor = $"{v.HullArmorFront}/{v.HullArmorSide}/{v.HullArmorRear}"
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

                Class = v.Class.ToString(),
                Level = v.Level,
                PurchaseCost = v.PurchaseCost,

                HP = v.HP,
                Damage = v.Damage,
                Penetration = v.Penetration,
                ReloadTime = v.ReloadTime,
                Accuracy = v.Accuracy,
                AimTime = v.AimTime,
                Speed = v.Speed,
                Acceleration = v.Acceleration,
                TraverseSpeed = v.TraverseSpeed,
                TurretTraverseSpeed = v.TurretTraverseSpeed,

                TurretArmor = $"{v.TurretArmorFront}/{v.TurretArmorSide}/{v.TurretArmorRear}",
                HullArmor = $"{v.HullArmorFront}/{v.HullArmorSide}/{v.HullArmorRear}"
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

                Class = v.Class.ToString(),
                Level = v.Level,
                PurchaseCost = v.PurchaseCost,

                HP = v.HP,
                Damage = v.Damage,
                Penetration = v.Penetration,
                ReloadTime = v.ReloadTime,
                Accuracy = v.Accuracy,
                AimTime = v.AimTime,
                Speed = v.Speed,
                Acceleration = v.Acceleration,
                TraverseSpeed = v.TraverseSpeed,
                TurretTraverseSpeed = v.TurretTraverseSpeed,

                TurretArmor = $"{v.TurretArmorFront}/{v.TurretArmorSide}/{v.TurretArmorRear}",
                HullArmor = $"{v.HullArmorFront}/{v.HullArmorSide}/{v.HullArmorRear}"
            });
        }

        // --- TECH TREE ---

        // GET /vehicles/{id}/research-from  -> хто може відкрити цей танк (і скільки XP треба на предку)
        [HttpGet("{id:int}/research-from")]
        public IActionResult GetResearchFrom(int id)
        {
            var links = _db.VehicleResearchRequirements
                .Where(r => r.SuccessorVehicleId == id)
                .Select(r => new
                {
                    predecessorId = r.PredecessorVehicleId,
                    requiredXp = r.RequiredXpOnPredecessor
                })
                .ToList();

            return Ok(links);
        }

        public class CreateLinkDto
        {
            public int PredecessorVehicleId { get; set; }
            public int SuccessorVehicleId { get; set; }
            public int RequiredXpOnPredecessor { get; set; }
        }

        // POST /vehicles/links
        [HttpPost("links")]
        public IActionResult CreateLink([FromBody] CreateLinkDto dto)
        {
            if (dto.PredecessorVehicleId == dto.SuccessorVehicleId)
            {
                return BadRequest("predecessor == successor");
            }

            bool ok = _db.Vehicles.Any(v => v.Id == dto.PredecessorVehicleId)
                   && _db.Vehicles.Any(v => v.Id == dto.SuccessorVehicleId);
            if (!ok) return NotFound("vehicle not found");

            bool dup = _db.VehicleResearchRequirements
                .Any(x => x.PredecessorVehicleId == dto.PredecessorVehicleId
                       && x.SuccessorVehicleId == dto.SuccessorVehicleId);
            if (dup) return Conflict("link exists");

            var link = new WarOfMachines.Models.VehicleResearchRequirement
            {
                PredecessorVehicleId = dto.PredecessorVehicleId,
                SuccessorVehicleId = dto.SuccessorVehicleId,
                RequiredXpOnPredecessor = dto.RequiredXpOnPredecessor
            };

            _db.VehicleResearchRequirements.Add(link);
            _db.SaveChanges();

            return Ok(new { link.Id });
        }

        // DELETE /vehicles/links/{id}
        [HttpDelete("links/{id:int}")]
        public IActionResult DeleteLink(int id)
        {
            var link = _db.VehicleResearchRequirements.FirstOrDefault(x => x.Id == id);
            if (link == null) return NotFound();

            _db.VehicleResearchRequirements.Remove(link);
            _db.SaveChanges();

            return NoContent();
        }
    }

    public class VehicleDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public string Branch { get; set; } = string.Empty;
        public string FactionCode { get; set; } = string.Empty;
        public string FactionName { get; set; } = string.Empty;

        public string Class { get; set; } = string.Empty;
        public int Level { get; set; }
        public int PurchaseCost { get; set; }

        public int HP { get; set; }
        public int Damage { get; set; }
        public int Penetration { get; set; }

        public float ReloadTime { get; set; }
        public float Accuracy { get; set; }
        public float AimTime { get; set; }

        public float Speed { get; set; }
        public float Acceleration { get; set; }
        public float TraverseSpeed { get; set; }
        public float TurretTraverseSpeed { get; set; }

        public string TurretArmor { get; set; } = "0/0/0";
        public string HullArmor { get; set; } = "0/0/0";
    }
}
