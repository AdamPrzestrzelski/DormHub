using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DormHub.Models
{
    public enum RoomStatus
    {
        Available,        // Dostępny
        Occupied,         // Zajęty
        UnderMaintenance  // W remoncie
    }

    public class RoomModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Numer pokoju jest wymagany")]
        public int RoomNumber { get; set; }

        [Required]
        public int BuildingId { get; set; }
        [ForeignKey("BuildingId")]
        public BuildingModel? Building { get; set; }

        [Required]
        public int TypeId { get; set; }
        [ForeignKey("TypeId")]
        public RoomTypeModel? RoomType { get; set; }

        [Range(0, 50)]
        public int Floor { get; set; } = 1;

        public RoomStatus Status { get; set; } = RoomStatus.Available;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public List<ResidentModel>? Residents { get; set; }
    }
}
