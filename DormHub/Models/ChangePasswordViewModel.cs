using System.ComponentModel.DataAnnotations;

namespace DormHub.Models
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Aktualne hasło jest wymagane")]
        [DataType(DataType.Password)]
        [Display(Name = "Aktualne hasło")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nowe hasło jest wymagane")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Nowe hasło musi mieć co najmniej 6 znaków")]
        [Display(Name = "Nowe hasło")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Potwierdzenie nowego hasła jest wymagane")]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Hasła nie są zgodne")]
        [Display(Name = "Potwierdź nowe hasło")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
