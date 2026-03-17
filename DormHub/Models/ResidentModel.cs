using System.ComponentModel.DataAnnotations;

namespace DormHub.Models
{
    public class ResidentModel : PersonModel
    {
        [Required(ErrorMessage = "Numer pokoju jest wymagany")]
        public string RoomNumber { get; set; }
        [Required(ErrorMessage = "Data wprowadzenia jest wymagana")]
        public DateTime MoveInDate { get; set; }
        public DateTime? MoveOutDate { get; set; }
    }
}
