using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarOfMachines.Data;
using WarOfMachines.Infrastructure;
using WarOfMachines.Models;

namespace WarOfMachines.Controllers;

[ApiController]
[Route("unity-server")]
public sealed class UnityServerStatusController : ControllerBase
{
    private static readonly TimeSpan DefaultHeartbeatTimeout = TimeSpan.FromSeconds(15);
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;

    public UnityServerStatusController(AppDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    [HttpGet("status")]
    [AllowAnonymous]
    public async Task<ActionResult<DedicatedServerStatusResponse>> GetStatus(CancellationToken cancellationToken)
    {
        DateTimeOffset cutoff = DateTimeOffset.UtcNow - GetHeartbeatTimeout();
        List<DedicatedServerStatusItem> servers = await _db.DedicatedServers
            .AsNoTracking()
            .Where(x => x.IsRegistered && x.LastHeartbeatUtc >= cutoff)
            .OrderBy(x => x.PlayersOnline)
            .ThenBy(x => x.ServerId)
            .Select(x => new DedicatedServerStatusItem
            {
                ServerId = x.ServerId,
                Region = x.Region,
                IsOnline = true,
                Status = x.Status,
                Address = x.Address,
                Port = x.Port,
                PlayersOnline = x.PlayersOnline,
                MaxPlayers = x.MaxPlayers,
                ActiveMatches = x.ActiveMatches,
                LastHeartbeatUtc = x.LastHeartbeatUtc,
                Message = x.Message
            })
            .ToListAsync(cancellationToken);

        return Ok(new DedicatedServerStatusResponse { Servers = servers });
    }

    [HttpPost("register")]
    [Authorize(Policy = "dedicatedServer")]
    public async Task<ActionResult<DedicatedServerRegistrationResponse>> Register(
        [FromBody] DedicatedServerRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        string? serverId = GetAuthenticatedServerId();
        if (serverId == null || !IsValidServerId(serverId))
        {
            return Forbid();
        }

        if (!TryNormalizeServerAddress(request.Address, out string address)
            || request.Port is < 1 or > 65535
            || request.MaxPlayers is < 1 or > 512)
        {
            return BadRequest("Invalid dedicated server registration.");
        }

        DedicatedServer? server = await _db.DedicatedServers
            .SingleOrDefaultAsync(x => x.ServerId == serverId, cancellationToken);

        if (server == null)
        {
            server = new DedicatedServer
            {
                ServerId = serverId,
                CredentialHash = DedicatedServerCredentialValidator.ComputeCredentialHash(
                    Request.Headers["X-Server-Credential"].ToString())
            };
            _db.DedicatedServers.Add(server);
        }

        ApplyRegistration(server, request, address);
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new DedicatedServerRegistrationResponse
        {
            ServerId = server.ServerId,
            HeartbeatIntervalSeconds = 5
        });
    }

    [HttpPut("{serverId}/heartbeat")]
    [Authorize(Policy = "dedicatedServer")]
    public async Task<ActionResult<DedicatedServerHeartbeatResponse>> Heartbeat(
        string serverId,
        [FromBody] DedicatedServerHeartbeatRequest request,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(serverId, GetAuthenticatedServerId(), StringComparison.Ordinal))
        {
            return Forbid();
        }

        DedicatedServer? server = await _db.DedicatedServers
            .SingleOrDefaultAsync(x => x.ServerId == serverId, cancellationToken);
        if (server == null)
        {
            return NotFound("Dedicated server is not registered.");
        }

        if (request.PlayersOnline is < 0 or > 512
            || request.MaxPlayers is < 1 or > 512
            || request.ActiveMatches is < 0 or > 1000
            || !TryNormalizeServerAddress(request.Address, out string address)
            || request.Port is < 1 or > 65535)
        {
            return BadRequest("Invalid heartbeat.");
        }

        server.Address = address;
        server.Port = request.Port;
        server.PlayersOnline = request.PlayersOnline;
        server.MaxPlayers = request.MaxPlayers;
        server.ActiveMatches = request.ActiveMatches;
        server.Status = NormalizeStatus(request.Status);
        server.Message = NormalizeMessage(request.Message);
        server.LastHeartbeatUtc = DateTimeOffset.UtcNow;
        server.IsRegistered = true;
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new DedicatedServerHeartbeatResponse(true, server.LastHeartbeatUtc));
    }

    [HttpPost("{serverId}/unregister")]
    [Authorize(Policy = "dedicatedServer")]
    public async Task<IActionResult> Unregister(string serverId, CancellationToken cancellationToken)
    {
        if (!string.Equals(serverId, GetAuthenticatedServerId(), StringComparison.Ordinal))
        {
            return Forbid();
        }

        DedicatedServer? server = await _db.DedicatedServers
            .SingleOrDefaultAsync(x => x.ServerId == serverId, cancellationToken);
        if (server == null)
        {
            return NotFound();
        }

        server.IsRegistered = false;
        server.Status = "offline";
        server.LastHeartbeatUtc = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private string? GetAuthenticatedServerId()
    {
        return User.FindFirstValue(DedicatedServerAuthenticationHandler.ServerIdClaim);
    }

    private TimeSpan GetHeartbeatTimeout()
    {
        int seconds = _configuration.GetValue("DedicatedServers:HeartbeatTimeoutSeconds", (int)DefaultHeartbeatTimeout.TotalSeconds);
        return TimeSpan.FromSeconds(Math.Clamp(seconds, 5, 300));
    }

    private static void ApplyRegistration(DedicatedServer server, DedicatedServerRegistrationRequest request, string address)
    {
        server.Region = request.Region.Trim();
        server.Address = address;
        server.Port = request.Port;
        server.MaxPlayers = request.MaxPlayers;
        server.PlayersOnline = 0;
        server.ActiveMatches = 0;
        server.Status = "ready";
        server.Message = null;
        server.LastHeartbeatUtc = DateTimeOffset.UtcNow;
        server.IsRegistered = true;
    }

    private static string NormalizeStatus(string? status)
    {
        string normalized = string.IsNullOrWhiteSpace(status) ? "online" : status.Trim().ToLowerInvariant();
        return normalized.Length > 32 ? normalized[..32] : normalized;
    }

    private static string? NormalizeMessage(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return null;
        }

        string normalized = message.Trim();
        return normalized.Length > 256 ? normalized[..256] : normalized;
    }

    private static bool IsValidServerId(string serverId)
    {
        return serverId.Length is >= 1 and <= 64
               && serverId.All(character => char.IsLetterOrDigit(character) || character is '-' or '_' or '.');
    }

    private static bool TryNormalizeServerAddress(string? value, out string address)
    {
        address = string.Empty;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string candidate = value.Trim();
        if (candidate.Length > 255 || candidate.Contains('/') || candidate.Contains(':') && System.Net.IPAddress.TryParse(candidate, out _) == false)
        {
            return false;
        }

        if (!System.Net.IPAddress.TryParse(candidate, out _)
            && Uri.CheckHostName(candidate) == UriHostNameType.Unknown)
        {
            return false;
        }

        address = candidate;
        return true;
    }
}

public sealed class DedicatedServerRegistrationRequest
{
    [Required]
    [StringLength(32, MinimumLength = 1)]
    public string Region { get; set; } = string.Empty;

    [Required]
    [StringLength(255, MinimumLength = 1)]
    public string Address { get; set; } = string.Empty;

    [Range(1, 65535)]
    public int Port { get; set; }

    [Range(1, 512)]
    public int MaxPlayers { get; set; }
}

public sealed class DedicatedServerHeartbeatRequest
{
    [Required]
    [StringLength(255, MinimumLength = 1)]
    public string Address { get; set; } = string.Empty;

    [Range(1, 65535)]
    public int Port { get; set; }

    [Range(0, 512)]
    public int PlayersOnline { get; set; }

    [Range(1, 512)]
    public int MaxPlayers { get; set; }

    [Range(0, 1000)]
    public int ActiveMatches { get; set; }

    [StringLength(32)]
    public string? Status { get; set; }

    [StringLength(256)]
    public string? Message { get; set; }
}

public sealed class DedicatedServerStatusResponse
{
    public List<DedicatedServerStatusItem> Servers { get; set; } = new();
}

public sealed class DedicatedServerStatusItem
{
    public string ServerId { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public bool IsOnline { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int Port { get; set; }
    public int PlayersOnline { get; set; }
    public int MaxPlayers { get; set; }
    public int ActiveMatches { get; set; }
    public DateTimeOffset LastHeartbeatUtc { get; set; }
    public string? Message { get; set; }
}

public sealed class DedicatedServerRegistrationResponse
{
    public string ServerId { get; set; } = string.Empty;
    public int HeartbeatIntervalSeconds { get; set; }
}

public sealed record DedicatedServerHeartbeatResponse(bool Ok, DateTimeOffset ReceivedAtUtc);
