using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WarOfMachines.Models
{
    public class VehicleResearchRequirement
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int PredecessorVehicleId { get; set; }

        [ForeignKey(nameof(PredecessorVehicleId))]
        public Vehicle? Predecessor { get; set; }

        [Required]
        public int SuccessorVehicleId { get; set; }

        [ForeignKey(nameof(SuccessorVehicleId))]
        public Vehicle? Successor { get; set; }

        [Range(0, int.MaxValue)]
        public int RequiredXpOnPredecessor { get; set; } = 0;
    }
}