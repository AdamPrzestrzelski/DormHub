using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DormHub.Models
{
    public class ResidentModel : PersonModel
    {
        [ForeignKey("RoomModel")]
        public int RoomId { get; set; }
        [Required(ErrorMessage = "Data wprowadzenia jest wymagana")]
        public DateTime MoveInDate { get; set; }
        public DateTime? MoveOutDate { get; set; }
    }
}
