namespace WarOfMachines.Infrastructure
{
    public sealed class UnityServerStatusStore
    {
        private static readonly TimeSpan OfflineAfter = TimeSpan.FromSeconds(6);

        private readonly object _lock = new();
        private UnityServerStatusSnapshot? _lastStatus;

        public UnityServerStatusSnapshot GetStatus()
        {
            lock (_lock)
            {
                if (_lastStatus == null)
                {
                    return UnityServerStatusSnapshot.Offline(null);
                }

                var now = DateTimeOffset.UtcNow;
                var lastHeartbeatUtc = _lastStatus.LastHeartbeatUtc ?? now;
                var timeSinceHeartbeat = now - lastHeartbeatUtc;
                var secondsSinceHeartbeat = Math.Max(0, (int)Math.Floor(timeSinceHeartbeat.TotalSeconds));
                bool isOnline = timeSinceHeartbeat <= OfflineAfter;

                return _lastStatus with
                {
                    IsOnline = isOnline,
                    SecondsSinceHeartbeat = secondsSinceHeartbeat
                };
            }
        }

        public UnityServerStatusSnapshot Update(UnityServerStatusUpdate update)
        {
            var now = DateTimeOffset.UtcNow;
            string status = string.IsNullOrWhiteSpace(update.Status) ? "online" : update.Status.Trim();

            var snapshot = new UnityServerStatusSnapshot(
                IsOnline: true,
                Status: status,
                LastHeartbeatUtc: now,
                SecondsSinceHeartbeat: 0,
                PlayersOnline: update.PlayersOnline,
                MaxPlayers: update.MaxPlayers,
                ActiveMatches: update.ActiveMatches,
                Message: string.IsNullOrWhiteSpace(update.Message) ? null : update.Message.Trim());

            lock (_lock)
            {
                _lastStatus = snapshot;
            }

            return snapshot;
        }
    }

    public sealed record UnityServerStatusUpdate
    {
        public string? Status { get; init; }
        public int? PlayersOnline { get; init; }
        public int? MaxPlayers { get; init; }
        public int? ActiveMatches { get; init; }
        public string? Message { get; init; }
    }

    public sealed record UnityServerStatusSnapshot(
        bool IsOnline,
        string Status,
        DateTimeOffset? LastHeartbeatUtc,
        int? SecondsSinceHeartbeat,
        int? PlayersOnline,
        int? MaxPlayers,
        int? ActiveMatches,
        string? Message)
    {
        public static UnityServerStatusSnapshot Offline(DateTimeOffset? lastHeartbeatUtc)
        {
            return new UnityServerStatusSnapshot(
                IsOnline: false,
                Status: "offline",
                LastHeartbeatUtc: lastHeartbeatUtc,
                SecondsSinceHeartbeat: null,
                PlayersOnline: null,
                MaxPlayers: null,
                ActiveMatches: null,
                Message: null);
        }
    }
}
