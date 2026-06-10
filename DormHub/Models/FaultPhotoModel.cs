using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DormHub.Models
{
    public class FaultPhotoModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int FaultId { get; set; }
        [ForeignKey("FaultId")]
        public FaultModel? Fault { get; set; }

        [Required]
        public byte[] Data { get; set; } = Array.Empty<byte>();

        [Required]
        [MaxLength(50)]
        public string ContentType { get; set; } = "image/jpeg";

        [MaxLength(260)]
        public string? FileName { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.Now;
    }
}
