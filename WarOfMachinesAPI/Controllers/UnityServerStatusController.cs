using Microsoft.AspNetCore.Mvc;
using WarOfMachines.Infrastructure;

namespace WarOfMachines.Controllers
{
    [ApiController]
    [Route("unity-server")]
    public sealed class UnityServerStatusController : ControllerBase
    {
        private readonly UnityServerStatusStore _statusStore;

        public UnityServerStatusController(UnityServerStatusStore statusStore)
        {
            _statusStore = statusStore;
        }

        [HttpGet("status")]
        public ActionResult<UnityServerStatusSnapshot> GetStatus()
        {
            return Ok(_statusStore.GetStatus());
        }

        [HttpPost("status")]
        public ActionResult<UnityServerStatusPostResponse> PostStatus([FromBody] UnityServerStatusUpdate update)
        {
            var status = _statusStore.Update(NormalizeStatusUpdate(update));
            return Ok(new UnityServerStatusPostResponse(true, status.LastHeartbeatUtc!.Value, status.IsOnline));
        }

        private UnityServerStatusUpdate NormalizeStatusUpdate(UnityServerStatusUpdate update)
        {
            string? address = NormalizeAddress(update.Address);

            if (address == null)
            {
                address = GetRequestAddress();
            }

            return update with
            {
                Address = address,
                Port = IsValidPort(update.Port) ? update.Port : null
            };
        }

        private string? GetRequestAddress()
        {
            string? address = NormalizeAddress(Request.Headers["X-Forwarded-For"].ToString());

            if (address != null)
            {
                return address;
            }

            address = NormalizeAddress(Request.Headers["X-Real-IP"].ToString());

            if (address != null)
            {
                return address;
            }

            return NormalizeAddress(HttpContext.Connection.RemoteIpAddress?.ToString());
        }

        private static string? NormalizeAddress(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            string address = value.Trim();
            int commaIndex = address.IndexOf(',');

            if (commaIndex >= 0)
            {
                address = address[..commaIndex].Trim();
            }

            if (address.Length == 0)
            {
                return null;
            }

            if (address[0] == '[')
            {
                int closingBracketIndex = address.IndexOf(']');

                if (closingBracketIndex > 0)
                {
                    address = address[1..closingBracketIndex].Trim();
                }
            }
            else
            {
                int lastColonIndex = address.LastIndexOf(':');

                if (lastColonIndex > 0 && address.IndexOf(':') == lastColonIndex && IsDigits(address, lastColonIndex + 1))
                {
                    address = address[..lastColonIndex].Trim();
                }
            }

            if (address.Length == 0)
            {
                return null;
            }

            if (System.Net.IPAddress.TryParse(address, out var ipAddress))
            {
                if (ipAddress.IsIPv4MappedToIPv6)
                {
                    ipAddress = ipAddress.MapToIPv4();
                }

                return ipAddress.ToString();
            }

            return address;
        }

        private static bool IsDigits(string value, int startIndex)
        {
            if (startIndex >= value.Length)
            {
                return false;
            }

            for (int i = startIndex; i < value.Length; i++)
            {
                if (value[i] < '0' || value[i] > '9')
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsValidPort(int? port)
        {
            if (!port.HasValue)
            {
                return false;
            }

            return port.Value >= 1 && port.Value <= 65535;
        }
    }

    public sealed record UnityServerStatusPostResponse(bool Ok, DateTimeOffset ReceivedAtUtc, bool IsOnline);
}
