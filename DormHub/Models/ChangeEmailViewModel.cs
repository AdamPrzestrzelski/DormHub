using System.ComponentModel.DataAnnotations;

namespace DormHub.Models
{
    public class ChangeEmailViewModel
    {
        [Required(ErrorMessage = "Nowy adres email jest wymagany")]
        [EmailAddress(ErrorMessage = "Błędny adres email")]
        [Display(Name = "Nowy e-mail (login)")]
        public string NewEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Potwierdzenie hasła jest wymagane")]
        [DataType(DataType.Password)]
        [Display(Name = "Aktualne hasło")]
        public string Password { get; set; } = string.Empty;
    }
}
