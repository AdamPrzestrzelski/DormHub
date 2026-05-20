using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DormHub.Models
{
    public enum PaymentStatus
    {
        Pending,  // Oczekująca
        Paid,     // Zapłacona
        Overdue   // Zaległa
    }

    public class PaymentModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ResidentId { get; set; }
        [ForeignKey("ResidentId")]
        public ResidentModel? Resident { get; set; }

        [Required(ErrorMessage = "Kwota jest wymagana")]
        [Range(0.01, 100000, ErrorMessage = "Kwota musi być większa od 0")]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Termin płatności jest wymagany")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? PaidAt { get; set; }

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        [MaxLength(200)]
        public string? Description { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
