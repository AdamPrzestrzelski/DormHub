using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DormHub.Models
{
    public class PaymentModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ResidentId { get; set; }
        [ForeignKey("ResidentId")]
        public ResidentModel? Resident { get; set; }

        [Required(ErrorMessage = "Kwota jest wymagana")]
        [Range(0.01, 100000, ErrorMessage = "Kwota musi byc wieksza od 0")]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Termin platnosci jest wymagany")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? PaidAt { get; set; }

        [Required]
        public int StatusId { get; set; } = 1;
        [ForeignKey("StatusId")]
        public PaymentStatusModel? Status { get; set; }

        [MaxLength(200)]
        public string? Description { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
