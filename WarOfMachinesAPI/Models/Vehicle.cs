using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace WarOfMachines.Models
{
    public class Vehicle
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Унікальний код техніки (глобально унікальний)
        [Required]
        public string Code { get; set; } = string.Empty;

        // Людинозрозуміла назва
        [Required]
        public string Name { get; set; } = string.Empty;

        // --- НОВЕ: Приналежність до фракції ---
        [Required]
        public int FactionId { get; set; }

        [ForeignKey(nameof(FactionId))]
        public Faction? Faction { get; set; }

        // --- НОВЕ: Гілка розвитку (тип шасі) ---
        // "tracked" | "biped" (на даному етапі 2 варіанти)
        [Required]
        public string Branch { get; set; } = "tracked";

        // Гнучкі стати однією JSONB-колонкою (hp, dmg, speed, armor, reload, тощо)
        [Column(TypeName = "jsonb")]
        public JsonDocument Stats { get; set; } = JsonDocument.Parse("""{}""");
    }
}