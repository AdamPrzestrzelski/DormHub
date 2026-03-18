using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DormHub.Models
{
    public class FaultModel
    {
        [Key]
        public int Id { get; set; } 

        [Required]
        public RoomModel Room { get; set; }

        [Required]
        public ResidentModel ReportedBy { get; set; }

        [Required(ErrorMessage = "Opis wymagany")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Data zgłoszenia wymagana")]
        public DateTime ReportedAt { get; set; }

        public bool IsResolved { get; set; } = false;

        public DateTime? ResolvedAt { get; set; }
    }
}
