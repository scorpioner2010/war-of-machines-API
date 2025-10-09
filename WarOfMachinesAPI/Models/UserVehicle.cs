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

        // 🔹 Гравець, якому належить техніка
        [Required]
        public int UserId { get; set; }

        // 🔹 Ідентифікатор конкретного робота/танка
        [Required]
        public int VehicleId { get; set; }

        // 🔹 Чи вибраний цей робот у профілі (активний)
        public bool IsActive { get; set; } = false;

        // 🔹 Досвід (XP), набраний саме цим роботом
        public int Xp { get; set; } = 0;

        // 🔹 Навігаційна властивість на гравця
        [ForeignKey(nameof(UserId))]
        public Player? User { get; set; }

        // 🔹 Навігаційна властивість на саму техніку
        [ForeignKey(nameof(VehicleId))]
        public Vehicle? Vehicle { get; set; }
    }
}