using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DormHub.Models
{
    public class RoomTypeModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Nazwa")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Pojemność")]
        public int Capacity { get; set; }

        [Required]
        [Display(Name = "Cena miesięczna (PLN)")]
        public decimal PricePerMonth { get; set; } = 0;
    }
}