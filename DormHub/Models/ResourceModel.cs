using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DormHub.Models
{
    public class ResourceModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa zasobu jest wymagana")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        // "Pralka","Suszarka","Zelazko","Odkurzacz","Inne"
        [Required(ErrorMessage = "Kategoria jest wymagana")]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        public int BuildingId { get; set; }
        [ForeignKey("BuildingId")]
        public BuildingModel? Building { get; set; }

        public List<ResourceBookingModel>? Bookings { get; set; }
    }
}
