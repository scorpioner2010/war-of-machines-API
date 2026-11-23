using System.ComponentModel.DataAnnotations;

namespace RavApi.Models;

public sealed class DedicatedServer
{
    [Key]
    [MaxLength(64)]
    public string ServerId { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string CredentialHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(32)]
    public string Region { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Address { get; set; } = string.Empty;

    public int Port { get; set; }

    public int MaxPlayers { get; set; }

    public int PlayersOnline { get; set; }

    public int ActiveMatches { get; set; }

    [Required]
    [MaxLength(32)]
    public string Status { get; set; } = "offline";

    [MaxLength(256)]
    public string? Message { get; set; }

    public DateTimeOffset LastHeartbeatUtc { get; set; }

    public bool IsRegistered { get; set; }
}
