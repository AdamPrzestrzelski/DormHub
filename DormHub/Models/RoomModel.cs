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

        [ForeignKey("BuildingModel")]
        public int BuildingId { get; set; }

        [ForeignKey("RoomTypeModel")]
        public int TypeId { get; set; }

        public List<ResidentModel>? Residents { get; set; }
    }
}
