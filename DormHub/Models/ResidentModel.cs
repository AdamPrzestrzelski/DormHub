using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DormHub.Models
{
    public class ResidentModel : PersonModel
    {
        [Required]
        public int? RoomId { get; set; }
        [ForeignKey("RoomId")]
        public RoomModel? Room { get; set; }

        [Required(ErrorMessage = "Data wprowadzenia jest wymagana")]
        public DateTime MoveInDate { get; set; }
        public DateTime? MoveOutDate { get; set; }
    }
}
