using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DormHub.Models
{
    public class ResourceBookingModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ResourceId { get; set; }
        [ForeignKey("ResourceId")]
        public ResourceModel? Resource { get; set; }

        [Required]
        public int ResidentId { get; set; }
        [ForeignKey("ResidentId")]
        public ResidentModel? Resident { get; set; }

        [Required(ErrorMessage = "Data rezerwacji jest wymagana")]
        [DataType(DataType.Date)]
        public DateTime BookingDate { get; set; }

        [Required(ErrorMessage = "Godzina początku jest wymagana")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "Godzina końca jest wymagana")]
        public TimeSpan EndTime { get; set; }

        [MaxLength(300)]
        public string? Notes { get; set; }

        public bool IsCancelled { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
