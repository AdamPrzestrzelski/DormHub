using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DormHub.Models
{
    public class RoomModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Numer pokoju jest wymagana")]
        public int RoomNumber { get; set; }

        [Required]
        public int BuildingId { get; set; }
        [ForeignKey("BuildingId")]
        public BuildingModel? Building { get; set; }

        [Required]
        public int TypeId { get; set; }
        [ForeignKey("TypeId")]
        public RoomTypeModel? RoomType { get; set; }

        public List<ResidentModel>? Residents { get; set; }
    }
}
