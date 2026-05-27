using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DormHub.Models
{
    [Table("Enum_ApplicationStatus")]
    public class ApplicationStatusModel
    {
        [Key] 
        public int Id { get; set; }

        [Required] 
        public string Name { get; set; } = string.Empty;

        public string? NameEn { get; set; }
    }

    [Table("Enum_PaymentStatus")]
    public class PaymentStatusModel
    {
        [Key] 
        public int Id { get; set; }

        [Required] 
        public string Name { get; set; } = string.Empty;

        public string? NameEn { get; set; }
    }

    [Table("Enum_RoomStatus")]
    public class RoomStatusModel
    {
        [Key] 
        public int Id { get; set; }

        [Required] 
        public string Name { get; set; } = string.Empty;

        public string? NameEn { get; set; }
    }

    [Table("Enum_FaultPriority")]
    public class FaultPriorityModel
    {
        [Key] 
        public int Id { get; set; }
        
        [Required] 
        public string Name { get; set; } = string.Empty;
        
        public string? NameEn { get; set; }
    }

    [Table("Enum_FaultCategory")]
    public class FaultCategoryModel
    {
        [Key] 
        public int Id { get; set; }
        
        [Required] 
        public string Name { get; set; } = string.Empty;
        
        public string? NameEn { get; set; }
    }

    
}
