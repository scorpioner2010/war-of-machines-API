using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WarOfMachines.Models
{
    public class UserVehicle
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Власник
        [Required]
        public int UserId { get; set; }

        // Посилання на каталог техніки
        [Required]
        public int VehicleId { get; set; }

        // Поточний активний вибір (у користувача має бути не більше 1 активного)
        public bool IsActive { get; set; } = false;

        // Навігаційні властивості
        [ForeignKey(nameof(UserId))]
        public Player? User { get; set; }

        [ForeignKey(nameof(VehicleId))]
        public Vehicle? Vehicle { get; set; }
    }
}