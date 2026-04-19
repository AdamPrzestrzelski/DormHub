using System.ComponentModel.DataAnnotations;

namespace DormHub.Models
{
    public class PersonModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Imię jest wymagane")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Nazwisko jest wymagane")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Data urodzenia jest wymagane")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateOnly DateOfBirth { get; set; }

        [Required(ErrorMessage = "Email jest wymagany")]
        [EmailAddress(ErrorMessage = "Błędny adres email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Numer telefonu jest wymagany")]
        [Phone(ErrorMessage = "Błędny numer telefonu")]
        public string PhoneNumber { get; set; }

        // --- dodatkowe atrybuty dla autoryzacji / uwierzytelniania ---
        // Hash hasła (format: base64(salt) + ":" + base64(hash))
        public string PasswordHash { get; set; }

        // Prosta rola (np. "Admin", "User"). Można rozszerzyć do kolekcji ról/claimów.
        public string Role { get; set; }

        // Dodatkowe pole bool pokazujące aktywność konta
        public bool IsActive { get; set; } = true;
    }
}
