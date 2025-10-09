using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WarOfMachines.Models
{
    [Table("UserVehicles")]
    public class UserVehicle
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int VehicleId { get; set; }

        [Required]
        public bool IsActive { get; set; } = false;

        [Required]
        [Range(0, int.MaxValue)]
        public int Xp { get; set; } = 0;

        [ForeignKey(nameof(UserId))]
        public virtual Player? User { get; set; }

        [ForeignKey(nameof(VehicleId))]
        public virtual Vehicle? Vehicle { get; set; }
    }
}