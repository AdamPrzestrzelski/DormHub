using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DormHub.Models
{
    public enum FaultPriority
    {
        Low,      // Niski
        Medium,   // Średni
        High,     // Wysoki
        Critical  // Krytyczny
    }

    public enum FaultCategory
    {
        Plumbing,    // Hydraulika
        Electrical,  // Elektryka
        Furniture,   // Meble
        Windows,     // Okna/Drzwi
        Internet,    // Internet/TV
        Other        // Inne
    }

    public class FaultModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RoomId { get; set; }
        [ForeignKey("RoomId")]
        public RoomModel? Room { get; set; }

        [Required]
        public int ReportedById { get; set; }
        [ForeignKey("ReportedById")]
        public ResidentModel? ReportedBy { get; set; }

        [Required(ErrorMessage = "Opis wymagany")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Data zgłoszenia wymagana")]
        [DataType(DataType.Date)]
        public DateTime ReportedAt { get; set; }

        public FaultPriority Priority { get; set; } = FaultPriority.Medium;

        public FaultCategory Category { get; set; } = FaultCategory.Other;

        public bool IsResolved { get; set; } = false;

        public DateTime? ResolvedAt { get; set; }

        [MaxLength(500)]
        public string? ResolutionNotes { get; set; }

        public int? ResolvedById { get; set; }
        [ForeignKey("ResolvedById")]
        public PersonModel? ResolvedBy { get; set; }
    }
}
