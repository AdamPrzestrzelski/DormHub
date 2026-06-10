using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DormHub.Models
{
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
        public PersonModel? ReportedBy { get; set; }

        [Required(ErrorMessage = "Opis wymagany")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Data zgloszenia wymagana")]
        [DataType(DataType.Date)]
        public DateTime ReportedAt { get; set; }

        [Required]
        public int PriorityId { get; set; } = 2;
        [ForeignKey("PriorityId")]
        public FaultPriorityModel? Priority { get; set; }

        [Required]
        public int CategoryId { get; set; } = 6;
        [ForeignKey("CategoryId")]
        public FaultCategoryModel? Category { get; set; }

        public bool IsResolved { get; set; } = false;

        public DateTime? ResolvedAt { get; set; }

        [MaxLength(500)]
        public string? ResolutionNotes { get; set; }

        public int? ResolvedById { get; set; }
        [ForeignKey("ResolvedById")]
        public PersonModel? ResolvedBy { get; set; }

        public ICollection<FaultPhotoModel> Photos { get; set; } = new List<FaultPhotoModel>();
    }
}
