using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WarOfMachines.Models
{
    public class Vehicle
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Code { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int FactionId { get; set; }

        [ForeignKey(nameof(FactionId))]
        public Faction? Faction { get; set; }

        [Required]
        public string Branch { get; set; } = "tracked"; // "tracked" | "biped"

        [Required]
        public VehicleClass Class { get; set; } = VehicleClass.Scout; // Scout|Guardian|Colossus

        [Range(1, 4)]
        public int Level { get; set; } = 1; // 1..4

        public int PurchaseCost { get; set; } = 0;

        public int HP { get; set; } = 0;
        public int Damage { get; set; } = 0;
        public int Penetration { get; set; } = 0;

        public float ReloadTime { get; set; } = 0f;
        public float Accuracy { get; set; } = 0f;
        public float AimTime { get; set; } = 0f;

        public float Speed { get; set; } = 0f;
        public float Acceleration { get; set; } = 0f;
        public float TraverseSpeed { get; set; } = 0f;
        public float TurretTraverseSpeed { get; set; } = 0f;

        public int TurretArmorFront { get; set; } = 0;
        public int TurretArmorSide { get; set; } = 0;
        public int TurretArmorRear { get; set; } = 0;

        public int HullArmorFront { get; set; } = 0;
        public int HullArmorSide { get; set; } = 0;
        public int HullArmorRear { get; set; } = 0;

        public ICollection<VehicleResearchRequirement> ResearchFrom { get; set; } = new List<VehicleResearchRequirement>();
        
        public bool IsVisible { get; set; } = true;
    }
}
