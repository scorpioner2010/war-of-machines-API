using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WarOfMachines.Data;

namespace WarOfMachines.Controllers
{
    [ApiController]
    [Route("leaderboard")]
    [Authorize]
    public class LeaderboardController : ControllerBase
    {
        private readonly AppDbContext _db;

        public LeaderboardController(AppDbContext db)
        {
            _db = db;
        }

        // GET /leaderboard/xp?top=10
        [HttpGet("xp")]
        public IActionResult GetByXp([FromQuery] int top = 10)
        {
            var list = _db.Players
                .OrderByDescending(p => p.XpTotal)
                .Take(top)
                .Select(p => new LeaderboardEntry
                {
                    UserId = p.Id,
                    Username = p.Username,
                    Value = p.XpTotal
                })
                .ToList();

            return Ok(list);
        }

        // GET /leaderboard/mmr?top=10
        [HttpGet("mmr")]
        public IActionResult GetByMmr([FromQuery] int top = 10)
        {
            var list = _db.Players
                .OrderByDescending(p => p.Mmr)
                .Take(top)
                .Select(p => new LeaderboardEntry
                {
                    UserId = p.Id,
                    Username = p.Username,
                    Value = p.Mmr
                })
                .ToList();

            return Ok(list);
        }
    }

    public class LeaderboardEntry
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}