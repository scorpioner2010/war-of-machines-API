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

        // Приналежність до фракції
        [Required]
        public int FactionId { get; set; }

        [ForeignKey(nameof(FactionId))]
        public Faction? Faction { get; set; }

        // Тип шасі: "tracked" | "biped"
        [Required]
        public string Branch { get; set; } = "tracked";

        // -------------------------------------------------
        // НОВІ СТАТИ (нормалізовані, без JSON)
        // -------------------------------------------------

        // Основні ТТХ
        public int HP { get; set; } = 0;
        public int Damage { get; set; } = 0;
        public int Penetration { get; set; } = 0;

        // Часи/точність (секунди/коеф.)
        public float ReloadTime { get; set; } = 0f;       // сек
        public float Accuracy { get; set; } = 0f;         // 0..1 або кут розсіювання — підберемо пізніше
        public float AimTime { get; set; } = 0f;          // сек

        // Мобільність
        public float Speed { get; set; } = 0f;            // макс. швидкість (м/с або ум.од.)
        public float Acceleration { get; set; } = 0f;     // прискорення (м/с^2 або ум.од.)
        public float TraverseSpeed { get; set; } = 0f;    // швидкість повороту корпусу (град/с)
        public float TurretTraverseSpeed { get; set; } = 0f; // швидкість повороту башти (град/с)

        // Броня (Front/Side/Rear)
        public int TurretArmorFront { get; set; } = 0;
        public int TurretArmorSide  { get; set; } = 0;
        public int TurretArmorRear  { get; set; } = 0;

        public int HullArmorFront { get; set; } = 0;
        public int HullArmorSide  { get; set; } = 0;
        public int HullArmorRear  { get; set; } = 0;
    }
}
