using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RavApi.Data;
using RavApi.Infrastructure;
using RavApi.Models;

namespace RavApi.Controllers
{
    [ApiController]
    [Route("auth")]
    [EnableRateLimiting("auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;
        private readonly JwtTokenService _jwtTokenService;

        public AuthController(
            AppDbContext db,
            ILogger<AuthController> logger,
            IConfiguration cfg,
            JwtTokenService jwtTokenService)
        {
            _db = db;
            _logger = logger;
            _configuration = cfg;
            _jwtTokenService = jwtTokenService;
        }

        public class RegisterRequest
        {
            [System.ComponentModel.DataAnnotations.Required]
            [System.ComponentModel.DataAnnotations.StringLength(32, MinimumLength = 3)]
            public string Username { get; set; } = "";

            [System.ComponentModel.DataAnnotations.Required]
            [System.ComponentModel.DataAnnotations.StringLength(128, MinimumLength = 8)]
            public string Password { get; set; } = "";
        }

        public class LoginRequest
        {
            [System.ComponentModel.DataAnnotations.Required]
            [System.ComponentModel.DataAnnotations.StringLength(32, MinimumLength = 1)]
            public string Username { get; set; } = "";

            [System.ComponentModel.DataAnnotations.Required]
            [System.ComponentModel.DataAnnotations.StringLength(128, MinimumLength = 1)]
            public string Password { get; set; } = "";
        }

        public class TokenResponse
        {
            public string Token { get; set; } = "";
            public string RefreshToken { get; set; } = "";
            public DateTime ExpiresAtUtc { get; set; }
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            string username = request.Username.Trim();

            bool exists = _db.Players.Any(p => p.Username == username);
            if (exists) return Conflict("Username already taken.");

            var player = new Player
            {
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                IsAdmin = false,
                CreatedAt = DateTimeOffset.UtcNow,
                Mmr = 0,
                FreeXp = 5000,
                Bolts = 50000,
                Adamant = 0
            };

            _db.Players.Add(player);
            _db.SaveChanges();

            EnsureStarterVehicle(player.Id);

            return Ok(CreateSession(player));
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            _logger.LogInformation("Login attempt: {Username}", request.Username);

            var user = _db.Players.FirstOrDefault(p => p.Username == request.Username);
            if (user == null) return Unauthorized("Invalid username or password.");

            bool ok = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!ok) return Unauthorized("Invalid username or password.");

            // auto-heal для старих юзерів без стартової техніки
            EnsureStarterVehicle(user.Id);

            return Ok(CreateSession(user));
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("auth")]
        public IActionResult Refresh([FromBody] RefreshRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return Unauthorized("Invalid refresh token.");
            }

            DateTimeOffset now = DateTimeOffset.UtcNow;
            var session = _db.UserSessions.SingleOrDefault(item =>
                item.RefreshTokenHash == ComputeTokenHash(request.RefreshToken));
            if (session == null || session.RevokedAtUtc != null || session.ExpiresAtUtc <= now)
            {
                return Unauthorized("Invalid refresh token.");
            }

            Player? user = _db.Players.SingleOrDefault(item => item.Id == session.UserId);
            if (user == null)
            {
                return Unauthorized("Invalid refresh token.");
            }

            string rotatedRefreshToken = CreateOpaqueToken();
            session.RefreshTokenHash = ComputeTokenHash(rotatedRefreshToken);
            session.LastUsedAtUtc = now;
            session.Version++;

            try
            {
                var accessToken = IssueJwt(user, session.Id);
                _db.SaveChanges();
                return Ok(new TokenResponse
                {
                    Token = accessToken.Token,
                    RefreshToken = rotatedRefreshToken,
                    ExpiresAtUtc = accessToken.ExpiresAtUtc
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                return Unauthorized("Invalid refresh token.");
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            if (!Guid.TryParse(User.FindFirstValue("sid"), out Guid sessionId))
            {
                return NoContent();
            }

            var session = _db.UserSessions.SingleOrDefault(item => item.Id == sessionId);
            if (session != null && session.RevokedAtUtc == null)
            {
                session.RevokedAtUtc = DateTimeOffset.UtcNow;
                session.Version++;
                _db.SaveChanges();
            }

            return NoContent();
        }

        public class RefreshRequest
        {
            [System.ComponentModel.DataAnnotations.Required]
            [System.ComponentModel.DataAnnotations.StringLength(512, MinimumLength = 20)]
            public string RefreshToken { get; set; } = "";
        }

        // ===== helpers =====

        private void EnsureStarterVehicle(int userId)
        {
            bool hasAny = _db.UserVehicles.Any(x => x.UserId == userId);
            if (hasAny) return;

            // 1) спробувати конкретний відомий код
            Vehicle? starter = _db.Vehicles.FirstOrDefault(v => v.Code == "ia_l1_starter");

            // 2) якщо ні — взяти будь-який видимий 1 рівня, або просто найдешевший
            starter ??= _db.Vehicles
                .OrderBy(v => v.Level)        // Level 1 насамперед
                .ThenByDescending(v => v.IsVisible)
                .ThenBy(v => v.PurchaseCost)
                .FirstOrDefault();

            if (starter == null) return; // у БД нема техніки — нічого не робимо

            var uv = new UserVehicle
            {
                UserId = userId,
                VehicleId = starter.Id,
                IsActive = true,
                Xp = 0
            };
            _db.UserVehicles.Add(uv);
            _db.SaveChanges();

            EnsureVehicleResearch(userId, starter.Id);
        }

        private void EnsureVehicleResearch(int userId, int vehicleId)
        {
            bool alreadyResearched = _db.UserVehicleResearches
                .Any(x => x.UserId == userId && x.VehicleId == vehicleId);
            if (alreadyResearched)
            {
                return;
            }

            _db.UserVehicleResearches.Add(new UserVehicleResearch
            {
                UserId = userId,
                VehicleId = vehicleId,
                ResearchedAt = DateTimeOffset.UtcNow
            });
            _db.SaveChanges();
        }

        private TokenResponse CreateSession(Player user)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            string refreshToken = CreateOpaqueToken();
            var session = new UserSession
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                RefreshTokenHash = ComputeTokenHash(refreshToken),
                CreatedAtUtc = now,
                LastUsedAtUtc = now,
                ExpiresAtUtc = now.AddDays(_configuration.GetValue("Authentication:RefreshTokenLifetimeDays", 30))
            };

            _db.UserSessions.Add(session);
            var accessToken = IssueJwt(user, session.Id);
            _db.SaveChanges();

            return new TokenResponse
            {
                Token = accessToken.Token,
                RefreshToken = refreshToken,
                ExpiresAtUtc = accessToken.ExpiresAtUtc
            };
        }

        private AccessTokenResult IssueJwt(Player user, Guid sessionId)
        {
            return _jwtTokenService.CreateAccessToken(user, sessionId);
        }

        private static string CreateOpaqueToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static string ComputeTokenHash(string token)
        {
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        }
    }
}
