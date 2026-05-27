using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DormHub.Models
{
    [Table("Enum_ApplicationStatus")]
    public class ApplicationStatusModel
    {
        [Key] 
        public int Id { get; set; }

        [Required][MaxLength(60)] 
        public string Name   { get; set; } = string.Empty;

        [MaxLength(60)]           
        public string? NameEn { get; set; }
    }

    [Table("Enum_PaymentStatus")]
    public class PaymentStatusModel
    {
        [Key] 
        public int Id { get; set; }

        [Required][MaxLength(60)] 
        public string Name   { get; set; } = string.Empty;

        [MaxLength(60)]           
        public string? NameEn { get; set; }
    }
}
