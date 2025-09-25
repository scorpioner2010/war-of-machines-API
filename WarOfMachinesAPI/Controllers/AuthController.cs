using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WarOfMachines.Data;
using WarOfMachines.Models;

namespace WarOfMachines.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AppDbContext db, IConfiguration config, ILogger<AuthController> logger)
        {
            _db = db;
            _config = config;
            _logger = logger;
        }

        // =======================
        // ======= DTOs ==========
        // =======================

        public class RegisterRequest
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public class LoginRequest
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public class AuthResponse
        {
            public string Token { get; set; } = string.Empty;
            public PlayerProfileDto Player { get; set; } = new PlayerProfileDto();
        }

        public class PlayerProfileDto
        {
            public int Id { get; set; }
            public string Username { get; set; } = string.Empty;
            public bool IsAdmin { get; set; }
            public int XpTotal { get; set; }
            public int Mmr { get; set; }
            public int Bolts { get; set; }
            public int Adamant { get; set; }

            public int? ActiveVehicleId { get; set; }
            public string? ActiveVehicleCode { get; set; }
            public string? ActiveVehicleName { get; set; }

            public OwnedVehicleDto[] OwnedVehicles { get; set; } = Array.Empty<OwnedVehicleDto>();
        }

        public class OwnedVehicleDto
        {
            public int VehicleId { get; set; }
            public string Code { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public bool IsActive { get; set; }
        }

        // =======================
        // ===== Endpoints =======
        // =======================

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            _logger.LogInformation("Register: {@Req}", request);

            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Username and password are required.");
            }

            if (_db.Players.Any(u => u.Username == request.Username))
            {
                return BadRequest("User already exists.");
            }

            var user = new Player
            {
                Username = request.Username.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                IsAdmin = false,
                XpTotal = 0,
                Mmr = 0,
                Bolts = 10000,  // стартові болти
                Adamant = 0     // стартовий адамант
            };

            _db.Players.Add(user);
            _db.SaveChanges();

            // Стартова техніка (starter) якщо є в каталозі
            string starterCode = _config["StarterVehicleCode"] ?? "starter";
            var starter = _db.Vehicles.FirstOrDefault(v => v.Code == starterCode);
            if (starter != null)
            {
                bool already = _db.UserVehicles.Any(x => x.UserId == user.Id && x.VehicleId == starter.Id);
                if (!already)
                {
                    _db.UserVehicles.Add(new UserVehicle
                    {
                        UserId = user.Id,
                        VehicleId = starter.Id,
                        IsActive = true
                    });
                    _db.SaveChanges();
                }
            }

            var token = CreateJwt(user);
            var profile = BuildProfile(user.Id);

            _logger.LogInformation("User {Username} registered.", user.Username);
            return Ok(new AuthResponse { Token = token, Player = profile });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            _logger.LogInformation("Login attempt: {Username}", request.Username);

            var user = _db.Players.FirstOrDefault(u => u.Username == request.Username);
            if (user == null)
            {
                _logger.LogWarning("Login failed: user not found.");
                return Unauthorized("Invalid username or password.");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed: wrong password.");
                return Unauthorized("Invalid username or password.");
            }

            var token = CreateJwt(user);
            var profile = BuildProfile(user.Id);

            _logger.LogInformation("User {Username} logged in.", user.Username);
            return Ok(new AuthResponse { Token = token, Player = profile });
        }

        // =======================
        // ===== Helpers =========
        // =======================

        private string CreateJwt(Player user)
        {
            var keyStr = _config["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(keyStr))
            {
                throw new InvalidOperationException("JWT Key not configured.");
            }

            var key = Encoding.UTF8.GetBytes(keyStr);
            var tokenHandler = new JwtSecurityTokenHandler();
            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim("isAdmin", user.IsAdmin.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(descriptor);
            return tokenHandler.WriteToken(token);
        }

        private PlayerProfileDto BuildProfile(int userId)
        {
            var u = _db.Players.First(x => x.Id == userId);

            var owned = _db.UserVehicles
                .Where(x => x.UserId == userId)
                .Include(x => x.Vehicle)
                .ToList();

            var active = owned.FirstOrDefault(x => x.IsActive);

            return new PlayerProfileDto
            {
                Id = u.Id,
                Username = u.Username,
                IsAdmin = u.IsAdmin,
                XpTotal = u.XpTotal,
                Mmr = u.Mmr,
                Bolts = u.Bolts,
                Adamant = u.Adamant,
                ActiveVehicleId = active?.VehicleId,
                ActiveVehicleCode = active?.Vehicle?.Code,
                ActiveVehicleName = active?.Vehicle?.Name,
                OwnedVehicles = owned
                    .Select(x => new OwnedVehicleDto
                    {
                        VehicleId = x.VehicleId,
                        Code = x.Vehicle != null ? x.Vehicle.Code : string.Empty,
                        Name = x.Vehicle != null ? x.Vehicle.Name : string.Empty,
                        IsActive = x.IsActive
                    })
                    .ToArray()
            };
        }
    }
}
