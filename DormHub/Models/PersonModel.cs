using System.ComponentModel.DataAnnotations;

namespace DormHub.Models
{
    public class PersonModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Imie jest wymagane")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nazwisko jest wymagane")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Data urodzenia jest wymagana")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateOnly DateOfBirth { get; set; }

        [Required(ErrorMessage = "Email jest wymagany")]
        [EmailAddress(ErrorMessage = "Bledny adres email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Numer telefonu jest wymagany")]
        [Phone(ErrorMessage = "Bledny numer telefonu")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Haslo jest wymagane")]
        public string PasswordHash { get; set; } = string.Empty;

        [Required(ErrorMessage = "Rola jest wymagana")]
        public string Role { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        [DataType(DataType.Date)]
        public DateTime? MoveinDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? MoveoutDate { get; set; }
    }
}
