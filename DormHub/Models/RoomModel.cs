using System.ComponentModel.DataAnnotations;

namespace DormHub.Models
{
    public class RoomModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Numer pokoju jest wymagana")]
        public int RoomNumber { get; set; }

        [Required(ErrorMessage = "Budynek jest wymagany")]
        public int Building { get; set; }

        [Required(ErrorMessage = "Należy podać ilu osobowy jest pokój")]
        public int Capacity { get; set; }

        public bool isDeluxe { get; set; } = false;

    }
}
