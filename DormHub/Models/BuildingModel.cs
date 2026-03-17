using System.ComponentModel.DataAnnotations;

namespace DormHub.Models
{
    public class BuildingModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa budynku wymagana")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Adres wymagany")]
        public string Address { get; set; }
    }
}
