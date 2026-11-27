using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RavApi.Data;
using RavApi.Infrastructure;
using RavApi.Models;

namespace RavApi.Controllers;

[ApiController]
[Route("connection-tickets")]
public sealed class ConnectionTicketsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly JwtTokenService _jwtTokenService;

    public ConnectionTicketsController(
        AppDbContext db,
        IConfiguration configuration,
        JwtTokenService jwtTokenService)
    {
        _db = db;
        _configuration = configuration;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ConnectionTicketResponse>> Issue(
        [FromBody] IssueConnectionTicketRequest request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue("sid"), out Guid sessionId)
            || !int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
        {
            return Unauthorized();
        }

        DedicatedServer? server = await _db.DedicatedServers
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.ServerId == request.ServerId, cancellationToken);
        if (server == null || !server.IsRegistered || server.LastHeartbeatUtc < DateTimeOffset.UtcNow - GetHeartbeatTimeout())
        {
            return NotFound("Dedicated server is not available.");
        }

        UserSession? session = await _db.UserSessions
            .SingleOrDefaultAsync(x => x.Id == sessionId && x.UserId == userId, cancellationToken);
        if (session == null || session.RevokedAtUtc != null || session.ExpiresAtUtc <= DateTimeOffset.UtcNow)
        {
            return Unauthorized();
        }

        string ticket = CreateOpaqueToken();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        var entity = new ConnectionTicket
        {
            Id = Guid.NewGuid(),
            TokenHash = ComputeHash(ticket),
            UserId = userId,
            SessionId = sessionId,
            ServerId = server.ServerId,
            IssuedAtUtc = now,
            ExpiresAtUtc = now.AddSeconds(GetTicketLifetimeSeconds())
        };

        _db.ConnectionTickets.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new ConnectionTicketResponse
        {
            Ticket = ticket,
            ServerId = server.ServerId,
            ExpiresAtUtc = entity.ExpiresAtUtc
        });
    }

    [HttpPost("redeem")]
    [Authorize(Policy = "dedicatedServer")]
    public async Task<ActionResult<ConnectionTicketRedemptionResponse>> Redeem(
        [FromBody] RedeemConnectionTicketRequest request,
        CancellationToken cancellationToken)
    {
        string? serverId = User.FindFirstValue(DedicatedServerAuthenticationHandler.ServerIdClaim);
        if (string.IsNullOrWhiteSpace(serverId))
        {
            return Forbid();
        }

        string tokenHash = ComputeHash(request.Ticket);
        var strategy = _db.Database.CreateExecutionStrategy();
        ConnectionTicketRedemptionResponse? redemption;
        try
        {
            redemption = await strategy.ExecuteAsync(async () =>
            {
                DateTimeOffset now = DateTimeOffset.UtcNow;
                await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

                ConnectionTicket? ticket = await _db.ConnectionTickets
                    .SingleOrDefaultAsync(x => x.TokenHash == tokenHash && x.ServerId == serverId, cancellationToken);
                if (ticket == null
                    || ticket.RedeemedAtUtc != null
                    || ticket.ExpiresAtUtc <= now)
                {
                    return null;
                }

                UserSession? session = await _db.UserSessions
                    .SingleOrDefaultAsync(x => x.Id == ticket.SessionId && x.UserId == ticket.UserId, cancellationToken);
                if (session == null || session.RevokedAtUtc != null || session.ExpiresAtUtc <= now)
                {
                    return null;
                }

                ticket.RedeemedAtUtc = now;
                ticket.Version++;
                await _db.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                var delegatedToken = await CreateDelegatedAccessToken(ticket.UserId, ticket.SessionId, cancellationToken);
                return new ConnectionTicketRedemptionResponse
                {
                    Accepted = true,
                    UserId = ticket.UserId,
                    SessionId = ticket.SessionId,
                    ServerId = serverId,
                    AccessToken = delegatedToken.Token,
                    AccessTokenExpiresAtUtc = delegatedToken.ExpiresAtUtc
                };
            });
        }
        catch (DbUpdateConcurrencyException)
        {
            return Unauthorized("Connection ticket is invalid.");
        }

        return redemption == null
            ? Unauthorized("Connection ticket is invalid.")
            : Ok(redemption);
    }

    [HttpPost("renew")]
    [Authorize(Policy = "dedicatedServer")]
    public async Task<ActionResult<DelegatedTokenRenewalResponse>> RenewDelegatedToken(
        [FromBody] DelegatedTokenRenewalRequest request,
        CancellationToken cancellationToken)
    {
        string? serverId = User.FindFirstValue(DedicatedServerAuthenticationHandler.ServerIdClaim);
        if (string.IsNullOrWhiteSpace(serverId) || request.SessionId == Guid.Empty)
        {
            return Forbid();
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;
        UserSession? session = await _db.UserSessions
            .SingleOrDefaultAsync(item => item.Id == request.SessionId, cancellationToken);
        if (session == null || session.RevokedAtUtc != null || session.ExpiresAtUtc <= now)
        {
            return Unauthorized("The player session is no longer active.");
        }

        bool sessionWasRedeemedByThisServer = await _db.ConnectionTickets
            .AsNoTracking()
            .AnyAsync(ticket => ticket.SessionId == session.Id
                && ticket.UserId == session.UserId
                && ticket.ServerId == serverId
                && ticket.RedeemedAtUtc != null,
                cancellationToken);
        if (!sessionWasRedeemedByThisServer)
        {
            return Forbid();
        }

        var token = await CreateDelegatedAccessToken(session.UserId, session.Id, cancellationToken);
        return Ok(new DelegatedTokenRenewalResponse
        {
            AccessToken = token.Token,
            AccessTokenExpiresAtUtc = token.ExpiresAtUtc
        });
    }

    private async Task<AccessTokenResult> CreateDelegatedAccessToken(int userId, Guid sessionId, CancellationToken cancellationToken)
    {
        Player user = await _db.Players.SingleAsync(item => item.Id == userId, cancellationToken);
        int lifetimeMinutes = Math.Clamp(
            _configuration.GetValue("Authentication:ServerDelegatedTokenMinutes", 20),
            5,
            20);
        return _jwtTokenService.CreateAccessToken(user, sessionId, lifetimeMinutes);
    }

    private int GetTicketLifetimeSeconds()
    {
        return Math.Clamp(_configuration.GetValue("Authentication:ConnectionTicketLifetimeSeconds", 30), 5, 120);
    }

    private TimeSpan GetHeartbeatTimeout()
    {
        int seconds = _configuration.GetValue("DedicatedServers:HeartbeatTimeoutSeconds", 15);
        return TimeSpan.FromSeconds(Math.Clamp(seconds, 5, 300));
    }

    private static string CreateOpaqueToken()
    {
        return Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
    }

    private static string ComputeHash(string value)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    }

    private static string Base64UrlEncode(byte[] data)
    {
        return Convert.ToBase64String(data)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}

public sealed class IssueConnectionTicketRequest
{
    [Required]
    [StringLength(64, MinimumLength = 1)]
    public string ServerId { get; set; } = string.Empty;
}

public sealed class RedeemConnectionTicketRequest
{
    [Required]
    [StringLength(512, MinimumLength = 20)]
    public string Ticket { get; set; } = string.Empty;
}

public sealed class ConnectionTicketResponse
{
    public string Ticket { get; set; } = string.Empty;
    public string ServerId { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAtUtc { get; set; }
}

public sealed class ConnectionTicketRedemptionResponse
{
    public bool Accepted { get; set; }
    public int UserId { get; set; }
    public Guid SessionId { get; set; }
    public string ServerId { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public DateTimeOffset AccessTokenExpiresAtUtc { get; set; }
}

public sealed class DelegatedTokenRenewalRequest
{
    [Required]
    public Guid SessionId { get; set; }
}

public sealed class DelegatedTokenRenewalResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTimeOffset AccessTokenExpiresAtUtc { get; set; }
}
