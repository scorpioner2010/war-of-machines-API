using System.ComponentModel.DataAnnotations;

namespace WarOfMachines.Models;

public sealed class ConnectionTicket
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(128)]
    public string TokenHash { get; set; } = string.Empty;

    public int UserId { get; set; }
    public Guid SessionId { get; set; }

    [Required]
    [MaxLength(64)]
    public string ServerId { get; set; } = string.Empty;

    public DateTimeOffset IssuedAtUtc { get; set; }
    public DateTimeOffset ExpiresAtUtc { get; set; }
    public DateTimeOffset? RedeemedAtUtc { get; set; }

    [ConcurrencyCheck]
    public int Version { get; set; }
}
