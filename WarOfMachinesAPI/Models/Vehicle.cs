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

        // Унікальний код для клієнта/балансу (наприклад: "light_mk1")
        [Required]
        public string Code { get; set; } = string.Empty;

        // Людинозрозуміла назва (наприклад: "Scout MK I")
        [Required]
        public string Name { get; set; } = string.Empty;

        // Гнучкі стати однією JSONB-колонкою (hp, dmg, speed, armor, reload, тощо)
        [Column(TypeName = "jsonb")]
        public JsonDocument Stats { get; set; } = JsonDocument.Parse("""{}""");
    }
}