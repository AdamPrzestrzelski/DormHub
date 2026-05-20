using System.ComponentModel.DataAnnotations;

namespace DormHub.Models
{
    public class RoomStatusModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(60)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(60)]
        public string? NameEn { get; set; }
    }
}
