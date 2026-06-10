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

    [Table("Enum_ApplicationType")]
    public class ApplicationTypeModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? NameEn { get; set; }
    }

    public static class ApplicationTypes
    {
        public const int Place = 1;            // Miejsce w akademiku (osoba bez przydziału)
        public const int RoomChange = 2;       // Zmiana pokoju (mieszkaniec)
        public const int SummerExtension = 3;  // Przedłużenie na wakacje (lipiec–październik)
        public const int NextYear = 4;         // Miejsce w nowym roku akademickim
        public const int Checkout = 5;         // Wymeldowanie
    }

    public static class ApplicationStatuses
    {
        public const int Pending = 1;
        public const int Accepted = 2;
        public const int Rejected = 3;
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
