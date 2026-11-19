using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using WarOfMachines.Data;

namespace WarOfMachines.Infrastructure;

public sealed class DedicatedServerCredentialValidator
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;

    public DedicatedServerCredentialValidator(AppDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    public async Task<bool> ValidateAsync(string serverId, string credential, CancellationToken cancellationToken)
    {
        if (serverId.Length > 64 || credential.Length > 256)
        {
            return false;
        }

        string credentialHash = ComputeCredentialHash(credential);
        var server = await _db.DedicatedServers
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.ServerId == serverId, cancellationToken);

        if (server != null)
        {
            return FixedTimeEquals(server.CredentialHash, credentialHash);
        }

        var provisioned = _configuration
            .GetSection("DedicatedServers:Provisioned")
            .Get<ProvisionedDedicatedServerOptions[]>() ?? Array.Empty<ProvisionedDedicatedServerOptions>();

        ProvisionedDedicatedServerOptions? entry = provisioned
            .FirstOrDefault(x => string.Equals(x.ServerId, serverId, StringComparison.Ordinal));

        return entry != null
               && !string.IsNullOrWhiteSpace(entry.Credential)
               && FixedTimeEquals(ComputeCredentialHash(entry.Credential), credentialHash);
    }

    public static string ComputeCredentialHash(string credential)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(credential)));
    }

    private static bool FixedTimeEquals(string expectedHex, string actualHex)
    {
        try
        {
            byte[] expected = Convert.FromHexString(expectedHex);
            byte[] actual = Convert.FromHexString(actualHex);
            return CryptographicOperations.FixedTimeEquals(expected, actual);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}

public sealed class ProvisionedDedicatedServerOptions
{
    public string ServerId { get; set; } = string.Empty;
    public string Credential { get; set; } = string.Empty;
}
