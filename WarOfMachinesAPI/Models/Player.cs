using System.Collections.Generic;

namespace WarOfMachines.Models
{
    public class Player
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public bool IsAdmin { get; set; } = false;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public int Mmr { get; set; } = 0;
        public int Bolts { get; set; } = 0;
        public int Adamant { get; set; } = 0;
        public int FreeXp { get; set; } = 0;

        // 🔹 ДОДАЙ ЦЕ:
        public ICollection<UserVehicle> UserVehicles { get; set; } = new List<UserVehicle>();
    }
}