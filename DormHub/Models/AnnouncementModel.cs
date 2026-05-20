using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DormHub.Models
{
    public class AnnouncementModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tytuł ogłoszenia jest wymagany")]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Treść ogłoszenia jest wymagana")]
        public string Content { get; set; }

        [Required]
        public int AuthorId { get; set; }
        [ForeignKey("AuthorId")]
        public PersonModel? Author { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [DataType(DataType.Date)]
        public DateTime? ExpiresAt { get; set; }

        public bool IsPinned { get; set; } = false;

        // null = dla wszystkich budynków
        public int? BuildingId { get; set; }
        [ForeignKey("BuildingId")]
        public BuildingModel? Building { get; set; }
    }
}
