using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DormHub.Models
{
    public class ResidentModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PersonId { get; set; }
        [ForeignKey("PersonId")]
        public PersonModel? Person { get; set; }
        public int? RoomId { get; set; }
        [ForeignKey("RoomId")]
        public RoomModel? Room { get; set; }

        [Required(ErrorMessage = "Data zameldowania jest wymagana")]
        [DataType(DataType.Date)]
        public DateTime MoveInDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? MoveOutDate { get; set; }
    }
}
