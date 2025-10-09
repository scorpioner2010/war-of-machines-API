using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WarOfMachines.Models
{
    public class Player
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public bool IsAdmin { get; set; } = false;

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        // --- Прогрес ---
        public int Mmr { get; set; } = 0;

        // 🔹 Free XP — універсальний досвід
        public int FreeXp { get; set; } = 0;

        // --- Валюти ---
        public int Bolts { get; set; } = 0;
        public int Adamant { get; set; } = 0;
    }
}