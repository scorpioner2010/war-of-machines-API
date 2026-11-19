using System.ComponentModel.DataAnnotations;

namespace WarOfMachines.Models;

public sealed class UserSession
{
    [Key]
    public Guid Id { get; set; }

    public int UserId { get; set; }

    [Required]
    [MaxLength(128)]
    public string RefreshTokenHash { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset ExpiresAtUtc { get; set; }
    public DateTimeOffset LastUsedAtUtc { get; set; }
    public DateTimeOffset? RevokedAtUtc { get; set; }

    [ConcurrencyCheck]
    public int Version { get; set; }
}
