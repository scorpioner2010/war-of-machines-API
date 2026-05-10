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
            var status = _statusStore.Update(update);
            return Ok(new UnityServerStatusPostResponse(true, status.LastHeartbeatUtc!.Value, status.IsOnline));
        }
    }

    public sealed record UnityServerStatusPostResponse(bool Ok, DateTimeOffset ReceivedAtUtc, bool IsOnline);
}
