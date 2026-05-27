using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DormHub.Models
{
    [Table("Enum_FaultPriority")]
    public class FaultPriorityModel
    {
        [Key] 
        public int Id { get; set; }
        
        [Required][MaxLength(60)] 
        public string Name   { get; set; } = string.Empty;
        
        [MaxLength(60)]           
        public string? NameEn { get; set; }
    }

    [Table("Enum_FaultCategory")]
    public class FaultCategoryModel
    {
        [Key] 
        public int Id { get; set; }
        
        [Required][MaxLength(60)] 
        public string Name   { get; set; } = string.Empty;
        
        [MaxLength(60)]           
        public string? NameEn { get; set; }
    }
}
